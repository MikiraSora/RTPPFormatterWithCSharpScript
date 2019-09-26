    using Microsoft.CSharp;
using RealTimePPDisplayer.Displayer;
using RealTimePPDisplayer.Formatter;
using Sync.Tools;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RTPPFormatterWithCSharpScript
{
    public class RtppCSSFormatter : FormatterBase, IFormatterClearable
    {
        private readonly static Logger Logger = new Logger<RtppCSSFormatter>();

        static RtppCSSFormatter()
        {
            try
            {
                using (var reader=new StreamReader(typeof(RtppCSSFormatter).Assembly.GetManifestResourceStream("RTPPFormatterWithCSharpScript.ScriptTemplate.cs")))
                {
                    var script_template = reader.ReadToEnd();
                    ScriptTemplate = script_template;
                }
            }
            catch (Exception e)
            {
                Logger.LogError("Can't get script template."+e.Message);
            }
        }

        private static string ScriptTemplate { get; } 

        private string format;
        public override string Format
        {
            get => format; set
            {
                format = value;
                UpdateFormat();
            }
        }

        Type cached_compiled_type;

        public Func<DisplayerBase, string> Executable { get; private set; }
        public Action Clearable { get; private set; }

        public RtppCSSFormatter(string format)
        {
            Format = format;
        }

        #region CodeDom

        private (Func<DisplayerBase,string> exec_func,Action clear_func) GenerateExecutableScriptContent(string source)
        {
            var compiler_params = new CompilerParameters();
            compiler_params.GenerateExecutable = false;
            compiler_params.CompilerOptions = "/optimize";

            foreach (string loc in AppDomain.CurrentDomain.GetAssemblies().Select(x => x?.Location).OfType<string>().Distinct().Where(x=>!string.IsNullOrEmpty(x)))
            {
                compiler_params.ReferencedAssemblies.Add(loc);
            }

            compiler_params.GenerateInMemory = false;
            compiler_params.IncludeDebugInformation = true;

            var source_code = PreformatSourcecode(source, ref compiler_params, out var _, out var rand_classname);

            using (var compiler = GetCompilerProvider())
            {
                var result = compiler.CompileAssemblyFromSource(compiler_params, source_code);

                if (result.Errors.HasErrors)
                {
                    foreach (CompilerError error in result.Errors)
                    {
                        Logger.LogError($"Compile source code failed:\nLine {error.Line}({error.ErrorNumber}):{error.ErrorText}");
                    }

                    return default;
                }

                var asm = result.CompiledAssembly;
                cached_compiled_type = asm.ExportedTypes.FirstOrDefault();
                cache_obj = asm.CreateInstance(cached_compiled_type.FullName);

                var method = cached_compiled_type.GetMethod("ExecuteWrapper");
                var exec_func = method.Invoke(cache_obj, new object[] { }) as Func<DisplayerBase, string>;

                method = cached_compiled_type.GetMethod("ClearWrapper");
                var clear_func = method?.Invoke(cache_obj, new object[] { }) as Action;

                return (exec_func,clear_func);
            }
        }

        private CodeDomProvider GetCompilerProvider()
        {
            return new Microsoft.CodeDom.Providers.DotNetCompilerPlatform.CSharpCodeProvider();
        }

        private string PreformatSourcecode(string source, ref CompilerParameters compiler_params, out string rand_namespace, out string rand_classname)
        {
            Random r = new Random();
            var rr = r.Next();
            rand_namespace = $"r{rr}namespace";
            rand_classname = $"r{rr}class";

            var assemblies_imports_regex = new Regex(@"//@(.*\.dll)", RegexOptions.Compiled);
            var using_namespace_imports_regex = new Regex(@"using [^\(\)\s\=]+;", RegexOptions.Compiled);
            var addition_regex = new Regex("@(.+?;)", RegexOptions.Compiled);
            var multi_line_addition_regex = new Regex("@@(.+?)@@", RegexOptions.Singleline);

            var source_code = ScriptTemplate;

            foreach (Match match in assemblies_imports_regex.Matches(source))
            {
                var loc = match.Groups[1].Value;
                compiler_params.ReferencedAssemblies.Add(loc);
                source = source.Replace(match.Value, string.Empty);
            }

            StringBuilder addition_builder = new StringBuilder();

            //add a wrapper if exist Clear()
            if (source.Contains("public void Clear()"))
                addition_builder.AppendLine("\npublic Action ClearWrapper()\n{\nreturn new Action(()=>Clear());\n}");

            foreach (Match match in addition_regex.Matches(source))
            {
                var content = match.Groups[1].Value;
                addition_builder.AppendLine(content);
                source = source.Replace(match.Value, string.Empty);
            }
            foreach (Match match in multi_line_addition_regex.Matches(source))
            {
                var content = match.Groups[1].Value;
                addition_builder.AppendLine(content);
                source = source.Replace(match.Value, string.Empty);
            }


            StringBuilder import_namespace_builder = new StringBuilder();
            foreach (Match match in using_namespace_imports_regex.Matches(source))
            {
                var val = match.Value;
                import_namespace_builder.AppendLine(val);
                source = source.Replace(val, string.Empty);
            }

            source_code = source_code.Replace("/*@@import@@*/", import_namespace_builder.ToString());
            source_code = source_code.Replace("/*@@script@@*/", source);
            source_code = source_code.Replace("/*@@class@@*/", rand_classname);
            source_code = source_code.Replace("/*@@namespace@@*/", rand_namespace);
            source_code = source_code.Replace("/*@@addition@@*/", addition_builder.ToString());

            Logger.LogInfomation($"\n--------------\n{source_code}\n--------------");

            return source_code;
        }

        #endregion

        private bool _exec_generating = false;
        private object cache_obj;

        private async void UpdateFormat()
        {
            if (!_exec_generating)
            {
                await Task.Run(() =>
                {
                    _exec_generating = true;
                    int format_source_code = default;

                    do
                    {
                        Logger.LogInfomation("UpdateFormat()");
                        format_source_code = Format.GetHashCode();

                        try
                        {
                            (Executable,Clearable) = GenerateExecutableScriptContent(Format);
                            
                        }
                        catch (Exception e)
                        {
                            Logger.LogError("Generate executable func failed." + e.Message);
                            Executable = null;
                        }

                    } while (format_source_code != Format.GetHashCode());

                    _exec_generating = false;

                });
            }
        }

        public void Clear()
        {
            try
            {
                /*
                 If there impl Clear() in script,just call them.
                 */
                if (Clearable!=null)
                {
                    Clearable?.Invoke();
                }
                else
                {
                    cache_obj = cached_compiled_type?.Assembly.CreateInstance(cached_compiled_type.Name);

                    var method = cached_compiled_type.GetMethod("ExecuteWrapper");
                    Executable = method.Invoke(cache_obj, new object[] { }) as Func<DisplayerBase, string>;
                }
            }
            catch (Exception e)
            {
                Logger.LogError($"Clean formatter error."+e.Message);
            }
        }

        public override string GetFormattedString()
        {
            return ExecuteFormat();
        }

        private string ExecuteFormat()
        {
            return Executable?.Invoke(Displayer);
        }
    }
}
