using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace WpfApplication7.Controls.RadioGroupClasses
{
    class RadioGroup:Grid
    {
        public List<RadioButton> RadioButtons;
        public int isCheckedIndex; 

        /*
        public RadioGroup()
        {
            RadioButtons = new List<RadioButton>();
        }

        
        public RadioGroup(List<RadioButton> thListRadioButtons)
        {
            RadioButtons = thListRadioButtons;
            ShowGridLines = true;
            int TheNumberOfButtons = RadioButtons.Capacity;

            foreach (RadioButton thRadioButton in thListRadioButtons)
            {
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                Children.Add(thRadioButton);
                SetRow(RadioButtons.Last(), RadioButtons.Capacity - TheNumberOfButtons);

                RadioButtons.Last().Checked += GroupRadio;
            }
        }
        */

        public RadioGroup(int TheNumberOfRadioButtons)
        {
            RadioButtons = new List<RadioButton>(TheNumberOfRadioButtons);

            for (; TheNumberOfRadioButtons > 0; TheNumberOfRadioButtons--)
            {
                RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                RadioButtons.Add(new RadioButton());
                RadioButtons.Last().Content = "Радиокнопка " + (RadioButtons.Capacity - TheNumberOfRadioButtons);
                Children.Add(RadioButtons.Last());
                SetRow(RadioButtons.Last(), RadioButtons.Capacity - TheNumberOfRadioButtons); 

                RadioButtons.Last().Checked += GroupRadio;
            }
            RadioButtons.First().IsChecked = true;
        }

        void GroupRadio(object sender, RoutedEventArgs e)
        {
            isCheckedIndex=RadioButtons.FindIndex(thRadioButton => thRadioButton.IsChecked.Value);
        }

    }
}
