using System.Windows;
using System.Windows.Interop;

namespace Ark.Views
{
    public partial class DisplayWindow : Window

    {
        //! ====================================================
        //! [+] DISPLAY WINDOW: everything starts here
        //! ====================================================
        public DisplayWindow()
        {
            //!? ====================================================
            //!? INITIALIZE: startup code goes here
            //!? ====================================================
            InitializeComponent();
        }

        //! ====================================================
        //! [+] INIT HWND: auto-hide window when first initialized
        //! ====================================================
        public void InitHwnd()
        {
            var helper = new WindowInteropHelper(this);
            helper.EnsureHandle();
        }
    }
}
