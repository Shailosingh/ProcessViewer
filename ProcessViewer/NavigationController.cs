using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ProcessViewer
{
    static class NavigationController
    {
        private static Frame? CurrentFrame;

        public static void SetFrame(Frame newFrame)
        {
            CurrentFrame = newFrame;
        }

        public static void NavigateToPage(object newPage)
        {
            if (CurrentFrame == null)
            {
                throw new NullReferenceException("Frame is not set");
            }

            CurrentFrame.Navigate(newPage);
        }
        
        public static void NavigateBackAndClearCurrentPageFromHistory()
        {
            if (CurrentFrame == null)
            {
                throw new NullReferenceException("Frame is not set");
            }

            if (CurrentFrame.NavigationService.CanGoBack)
            {
                CurrentFrame.NavigationService.GoBack();
                CurrentFrame.NavigationService.RemoveBackEntry();
            }

        }
    }
}
