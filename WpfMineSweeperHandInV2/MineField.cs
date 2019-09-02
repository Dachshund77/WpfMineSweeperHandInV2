using IX.Observable;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace WpfMineSweeperHandInV2
{

    public enum CardinalDirection
    {
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }

    class MineField : Button, INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;


        MineSweeperGame parentGame; //TODO remove this reference, not needed.
        private bool c_isEnabled; // WHY IS THAT NOT VIRTUAL! c for custom casue why not
        private bool containsMine = false;
        private bool suspectedMine = false;

        public MineField(MineSweeperGame parentGame)
        {
            /*
             * I never realy asked but, init paramters in constructor vs in class. 
             * Normaly i always init stuff in the constructor, to make it more readable but there is 
             * a case to be made to create less repeating code by defining them in the class...?
             */
            this.parentGame = parentGame;
            this.Neigbours = new ObservableDictionary<CardinalDirection, MineField>(8);
            this.c_isEnabled = base.IsEnabled;         

            //setup Bindings 
            MultiBinding multiBinding = new MultiBinding();
            multiBinding.Converter = new MineFieldMultiConverter();

            //First condition (SuspectedMine)           
            Binding binding1 = new Binding();
            binding1.Source = this;
            binding1.Path = new PropertyPath("SuspectedMine");
            binding1.Mode = BindingMode.OneWay;


            //Second condition (ContainsMine)
            Binding binding2 = new Binding();
            binding2.Source = this;
            binding2.Path = new PropertyPath("ContainsMine");
            binding2.Mode = BindingMode.OneWay;

            //Third condition (IsEnabled)
            Binding binding3 = new Binding();
            binding3.Source = this;
            binding3.Path = new PropertyPath("IsEnabled");
            binding3.Mode = BindingMode.OneWay;

            //Fourth condition (no Of neigbour mines)
            Binding binding4 = new Binding();
            binding4.Source = this;
            binding4.Path = new PropertyPath("Neigbours"); //This should in theory listen to add/delete... etc
            binding4.Mode = BindingMode.OneWay;

            //Listening to to the mineField inside the observable?

            //Finalise binding
            multiBinding.Bindings.Add(binding1);
            multiBinding.Bindings.Add(binding2);
            multiBinding.Bindings.Add(binding3);
            multiBinding.Bindings.Add(binding4);

            this.SetBinding(ContentProperty, multiBinding);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e) //TODO fix the bug of the "recursive call"
        {
            if (!suspectedMine && IsEnabled)
            {
                IsEnabled = false;

                //Test if no mines adjacent
                bool noMinesAdjacent = true;
                foreach (KeyValuePair<CardinalDirection, MineField> item in Neigbours)
                {
                    MineField tempMineField = item.Value;
                    if (tempMineField.containsMine || tempMineField.suspectedMine)
                    {
                        noMinesAdjacent = false;
                        break;
                    }
                }

                if (noMinesAdjacent)
                {
                    foreach (KeyValuePair<CardinalDirection, MineField> item in Neigbours)
                    {
                        MineField tempMineField = item.Value;
                        tempMineField.OnMouseLeftButtonDown(e);
                    }
                }
            }
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseRightButtonDown(MouseButtonEventArgs e)
        {
            if (SuspectedMine)
            {
                SuspectedMine = false;
            }
            else
            {
                SuspectedMine = true;
            }           
            base.OnMouseRightButtonDown(e);

        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Middle)
            {

                OnMouseLeftButtonDown(e);
                foreach (KeyValuePair<CardinalDirection, MineField> item in Neigbours) 
                {
                    if (item.Value.c_isEnabled)
                    {
                        item.Value.OnMouseLeftButtonDown(e);
                    }
                }

                e.Handled = true; //With that we can catch middle mouse event and DONT evoke other events
            }
            base.OnMouseDown(e);

        }

        private void PropertyIsChanged([CallerMemberName] String memberName = "")
        {

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(memberName)); //I would realy like to add state changed too here
        }

        public bool SuspectedMine
        {
            get { return suspectedMine; }
            set
            {
                suspectedMine = value;
                PropertyIsChanged();
            }
        }

        public new bool IsEnabled
        {
            get { return base.IsEnabled; }
            set
            {
                c_isEnabled = value;
                base.IsEnabled = c_isEnabled; //That probably important somewhere
                PropertyIsChanged();
            }
        }

        public ObservableDictionary<CardinalDirection, MineField> Neigbours { get; }

        public bool ContainsMine {
            get { return containsMine; }
            set { containsMine = value;
                PropertyIsChanged();
            }
        } 

    }

    public class MineFieldMultiConverter : IMultiValueConverter //Can we covert this to a Anon class?
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {

            //Failsafe against unset values
            if (values[0] == DependencyProperty.UnsetValue || values[1] == DependencyProperty.UnsetValue || values[2] == DependencyProperty.UnsetValue || values[3] == DependencyProperty.UnsetValue)
            {
                return "";
            }

            //Convert needed values
            bool suspectedMine = (bool)values[0];
            bool containsMine = (bool)values[1];
            bool isEnabled = (bool)values[2];
            ObservableDictionary<CardinalDirection, MineField> neighbours = (ObservableDictionary<CardinalDirection, MineField>)values[3];

            //Implement logic
            if (isEnabled)
            {
                if (suspectedMine)
                {
                    return "F";
                }
                else
                {
                    return "";
                }
            }
            else
            {
                if (containsMine)
                {
                    return "M";
                }
                else
                {
                    int neighboursWithMines = 0;
                    foreach (KeyValuePair<CardinalDirection, MineField> item in neighbours)
                    {
                        MineField tempMineField = item.Value;
                        if (tempMineField.ContainsMine)
                        {
                            neighboursWithMines++;
                        }
                    }
                    if (neighboursWithMines == 0)
                    {
                        return "";
                    }
                    else
                    {
                        return neighboursWithMines;
                    }
                }
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
