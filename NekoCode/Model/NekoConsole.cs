using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
namespace NekoCode
{
    
    public class NekoConsole
    {
        StackPanel OutputStackPanel;
        public MainWindow MainWindow;
        public bool IsEnabled = true;
        public NekoConsole(StackPanel output, MainWindow mainWindow) 
        {
            OutputStackPanel = output;
            MainWindow = mainWindow;
        }
        public void WriteLine(dynamic text)
        {
            if (!IsEnabled) return;
            MainWindow.WriteToMainConsole(OutputStackPanel, text.ToString());
        }
        public void Write(dynamic text) 
        {
            if (!IsEnabled) return;
            dynamic line = OutputStackPanel.Children[OutputStackPanel.Children.Count - 1];
            if (line.GetType() == typeof(string))
            {
                line += text;
            }
            OutputStackPanel.Children[OutputStackPanel.Children.Count - 1] = line;
        }

        public void RichWriteLine(dynamic[] items)
        {
            if (!IsEnabled) return;
            Console.WriteLine($"RichWriteLine Items: {items.Length}");
            WrapPanel grid = new WrapPanel();
            
            grid.VerticalAlignment = VerticalAlignment.Stretch;
            grid.HorizontalAlignment = HorizontalAlignment.Stretch;
            foreach (dynamic item in items)
            {
                if (item.GetType() == typeof(Button))
                {
                    item.Style = MainWindow.CurrentTheme["ButtonStyle"] as Style;
                }
                grid.Children.Add(new Grid() { Margin = new Thickness(5,0,0,0) });
                grid.Children.Add(MainWindow.ConvertDataForOutput(item));
            }
            grid.Style = MainWindow.CurrentTheme["WrapPanelStyle"] as Style;
            MainWindow.AddToConsole(OutputStackPanel, grid);
        }
        public void Clear()
        {
            if (!IsEnabled) return;
            OutputStackPanel.Children.Clear();
        }
        public string ReadLine(string message="", string text="")
        {
            if (!IsEnabled) return "1";
            return MainWindow.Input(message, text);
        }
    }
}
