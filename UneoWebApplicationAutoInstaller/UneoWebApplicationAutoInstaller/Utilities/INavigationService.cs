using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace UneoWebApplicationAutoInstaller.Utilities
{
    public interface INavigationService
    {
        void NavigateTo(Page page);
    }
    public class FrameNavigationService : INavigationService
    {
        private readonly Frame _frame;

        public FrameNavigationService(Frame frame)
        {
            _frame = frame;
        }

        public void NavigateTo(Page page)
        {
            _frame.Navigate(page);
        }
    }

}
