using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvaloniaDemo.ViewModels
{
    public class DesignerContext
    {
        //public static ProjectEditorViewModel Editor { get; set; }

        //public static ScenarioContainerViewModel Scenario { get; set; }

        public static void InitializeContext()
        {
            var begin = DateTime.Now;
            var duration = TimeSpan.FromDays(1);
        }
    }
}
