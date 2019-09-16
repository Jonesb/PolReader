using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PolReader
{
    /// <summary>
    /// Interaction logic for ProgressView.xaml
    /// </summary>
    public partial class ProgressView : Window
    {
        public delegate void UpdateDelegate(int Total,int Count);
        public UpdateDelegate UpdateDelegateMethodInstance;

        public delegate void UpdateDetailDelegate(string Detail);
        public UpdateDetailDelegate UpdateDetailDelegateMethodInstance;

        public delegate void ResetDelegate();
        public ResetDelegate ResetDelegateMethodInstance;
        
        public ProgressView()
        {
            InitializeComponent();

            UpdateDelegateMethodInstance = new UpdateDelegate(UpdateMethod);
            UpdateDetailDelegateMethodInstance = new UpdateDetailDelegate(UpdateDetailMethod);
            ResetDelegateMethodInstance = new ResetDelegate(ResetMethod);
        }

        public void ResetMethod()
        {            
            ProgBar.Minimum = 0;
            ProgBar.Maximum = 1;
            ProgBar.Value = 0;

            DetailText.Text = "Starting......";
            InfoText.Text = "";
        }

        public void UpdateMethod(int Total, int Count)
        {
            ProgBar.Minimum = 0;
            ProgBar.Maximum = Total;
            ProgBar.Value = Count;

            InfoText.Text = Count + " Of " + Total; 
        }

        public void UpdateDetailMethod(string Detail)
        {
            DetailText.Text = Detail;
        }

        public void UpdateDetail(string Detail)
        {
            this.Dispatcher.Invoke(UpdateDetailDelegateMethodInstance, new object[] { Detail });
        }

        public void Reset()
        {
            this.Dispatcher.Invoke(ResetDelegateMethodInstance, null);
        }

        public void Update(int Total, int Count)
        {
            this.Dispatcher.Invoke(UpdateDelegateMethodInstance, new object[] {Total, Count });            
        }
    }
}
