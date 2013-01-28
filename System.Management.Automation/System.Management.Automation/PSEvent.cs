namespace System.Management.Automation
{
    using System;
    using System.Reflection;
    using System.Text;

    public class PSEvent : PSMemberInfo
    {
        internal EventInfo baseEvent;

        internal PSEvent(EventInfo baseEvent)
        {
            this.baseEvent = baseEvent;
            base.name = baseEvent.Name;
        }

        public override PSMemberInfo Copy()
        {
            PSEvent destiny = new PSEvent(this.baseEvent);
            base.CloneBaseProperties(destiny);
            return destiny;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(this.baseEvent.ToString());
            builder.Append("(");
            int num = 0;
            foreach (ParameterInfo info in this.baseEvent.EventHandlerType.GetMethod("Invoke").GetParameters())
            {
                if (num > 0)
                {
                    builder.Append(", ");
                }
                builder.Append(info.ParameterType.ToString());
                num++;
            }
            builder.Append(")");
            return builder.ToString();
        }

        public override PSMemberTypes MemberType
        {
            get
            {
                return PSMemberTypes.Event;
            }
        }

        public override string TypeNameOfValue
        {
            get
            {
                return typeof(PSEvent).FullName;
            }
        }

        public sealed override object Value
        {
            get
            {
                return this.baseEvent;
            }
            set
            {
                throw new ExtendedTypeSystemException("CannotChangePSEventInfoValue", null, ExtendedTypeSystem.CannotSetValueForMemberType, new object[] { base.GetType().FullName });
            }
        }
    }
}

