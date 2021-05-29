using Microsoft.CSharp.Activities;
using Microsoft.VisualBasic.Activities;
using System;
using System.Activities;
using System.IO;
using System.Linq;

namespace Autossential.Shared
{
    public sealed class ExpressionServiceLanguage
    {
        private ExpressionServiceLanguage()
        {
            Language = Language.VisualBasic;

            DetectLanguageFromConfigFile();
        }

        private const string CONFIG_FILE_NAME = "Autossential.config";

        private void DetectLanguageFromConfigFile()
        {
            string config = null;
            var baseDir = Environment.CurrentDirectory;
            var root = Path.GetPathRoot(baseDir);

            while (config == null)
            {
                config = FindConfig(baseDir);
                if (baseDir == root)
                    break;

                baseDir = Path.GetFullPath(Path.Combine(baseDir, "../"));
            }

            if (config != null)
            {
                try
                {
                    foreach (var data in File.ReadAllLines(config).Where(line => line.Trim() != string.Empty))
                    {
                        var pair = data.Split('=');
                        if (pair[0].Equals("Language", StringComparison.OrdinalIgnoreCase))
                        {
                            if (pair[1] == "C#") pair[1] = "CSharp";

                            Language = (Language)Enum.Parse(typeof(Language), pair[1]);
                        }
                    }
                }
                catch { }
            }
        }

        private string FindConfig(string dir)
        {
            var config = Path.Combine(dir, CONFIG_FILE_NAME);
            if (File.Exists(config))
                return config;

            return null;
        }

        private static readonly Lazy<ExpressionServiceLanguage> _instance = new Lazy<ExpressionServiceLanguage>(() => new ExpressionServiceLanguage());

        public static ExpressionServiceLanguage Current => _instance.Value;

        public Language Language { get; private set; }

        public CodeActivity<T> CreateExpression<T>(string expression) => CreateExpression<T>(expression, expression);

        public CodeActivity<T> CreateExpression<T>(string vbExpression, string csExpression)
        {
            if (Language == Language.CSharp)
                return new CSharpValue<T>(csExpression);

            return new VisualBasicValue<T>(vbExpression);
        }
    }

    public enum Language
    {
        CSharp,
        VisualBasic
    }
}