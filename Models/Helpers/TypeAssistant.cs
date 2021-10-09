using System;
using System.Threading;

namespace Ark.Models.Helpers
{
    //! ====================================================
    //! TYPE ASSISTANT: helps me with the delayed textbox thingy
    //! https://stackoverflow.com/questions/33776387/dont-raise-textchanged-while-continuous-typing/33777265
    //! ====================================================
    public class TypeAssistant
    {
        public event EventHandler Idled = delegate { };
        public int WaitingMilliSeconds { get; set; }
        System.Threading.Timer waitingTimer;

        public TypeAssistant(int waitingMilliSeconds = 500)
        {
            WaitingMilliSeconds = waitingMilliSeconds;
            waitingTimer = new Timer(p =>
            {
                Idled(this, EventArgs.Empty);
            });
        }
        public void TextChanged()
        {
            waitingTimer.Change(WaitingMilliSeconds, System.Threading.Timeout.Infinite);
        }
    }
}