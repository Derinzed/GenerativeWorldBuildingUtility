using JohnUtilities.Model.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeWorldBuildingUtility.Model
{

    public class GeneratorBaseEvents
    {
        public void SetupEvents() {
            FieldInfo[] members = this.GetType().GetFields();
            foreach (FieldInfo member in members)
            {
                var obj = (EventType)member.GetValue(this);
                EventReporting.GetEventReporter().CreateEventType(obj);
            }
        }
        public static readonly EventType Startup = new EventType();
        public static readonly EventType Close = new EventType();
        public static readonly EventType PromptExecuted = new EventType();
        public static readonly EventType PromptCompleted = new EventType();
    }
}
