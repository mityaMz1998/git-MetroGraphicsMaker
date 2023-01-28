using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;


namespace WpfApplication7.Controls.RadioGroupClasses
{
    class GroupRadioGroups : Grid
    {
        public List<RadioGroup> RadioGroups;

        public int MaximumNumberOfVariant = 0;
        public int NumberOfVariant = 0;

        public GroupRadioGroups()
        {
            RadioGroups = new List<RadioGroup>();
        }

        public GroupRadioGroups(List<int> _TheNumbersOfRadioButtons)
        {
            List<int> TheNumbersOfRadioButtons = new List<int>(_TheNumbersOfRadioButtons.Count);
            foreach (int thNumberOfRadioButtons in _TheNumbersOfRadioButtons) if (thNumberOfRadioButtons > 0)
                {
                    TheNumbersOfRadioButtons.Add(thNumberOfRadioButtons);
                }
            if (TheNumbersOfRadioButtons.Count > 0)
            {
                MaximumNumberOfVariant = 1;
            }
            RadioGroups = new List<RadioGroup>(TheNumbersOfRadioButtons.Count);


            for (int Index = TheNumbersOfRadioButtons.Count; Index > 0; Index--)
            {
                MaximumNumberOfVariant = MaximumNumberOfVariant * TheNumbersOfRadioButtons[Index-1];
                ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                RadioGroups.Add(new RadioGroup(TheNumbersOfRadioButtons[Index - 1]));
                foreach (RadioButton thRadioButton in RadioGroups.Last().RadioButtons) thRadioButton.Checked += CalcСhoice;
                Children.Add(RadioGroups.Last());
                SetColumn(RadioGroups.Last(), RadioGroups.Capacity - Index);
            }
        }

        public void CalcСhoice(object sender, RoutedEventArgs e)
        {
            NumberOfVariant=0;
            int TheDimensionOfASet = 1;
            for (int Number = RadioGroups.Count-1; Number >= 0; Number--)
            {
                NumberOfVariant = NumberOfVariant + RadioGroups[Number].isCheckedIndex * TheDimensionOfASet;
                TheDimensionOfASet = TheDimensionOfASet * RadioGroups[Number].RadioButtons.Count;
            }
        }
    }
}
