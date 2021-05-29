using Autossential.Shared.Activities.Localization;
using Autossential.Activities.Properties;
using Autossential.Enums;
using System.Activities;

namespace Autossential.Activities
{
    public sealed class Stopwatch : CodeActivity
    {
        public InOutArgument<System.Diagnostics.Stopwatch> StopwatchObj { get; set; }

        [LocalCateg(nameof(Resources.Options_Category))]
        public StopwatchMethods Method { get; set; }

        protected override void Execute(CodeActivityContext context)
        {
            var obj = StopwatchObj.Get(context) ?? new System.Diagnostics.Stopwatch();

            switch (Method)
            {
                case StopwatchMethods.Start:
                    obj.Start();
                    break;

                case StopwatchMethods.Stop:
                    obj.Stop();
                    break;

                case StopwatchMethods.Reset:
                    obj.Reset();
                    break;

                case StopwatchMethods.Restart:
                    obj.Restart();
                    break;
            }

            StopwatchObj.Set(context, obj);
        }
    }
}