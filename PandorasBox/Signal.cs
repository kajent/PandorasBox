using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;

namespace PandorasBox
{
    class Signal//: IComparer
    {
        public Utilities.Command signal;
        public int dayMod;
        public int date;
        public double price;
        public string symbol;
        public string exchange;

        public Signal(Utilities.Command Signal, int Day, int Date, double Price, string Exchange, string Symbol)
        {
            signal = Signal;
            dayMod = Day;
            date = Date;
            price = Price;
            exchange = Exchange;
            symbol = Symbol;
        }

        /*
        int IComparer.Compare(object obj1, object obj2)
        {
            Signal s1 = (Signal)obj1;
            Signal s2 = (Signal)obj2;
            return s1.dayMod.CompareTo(s2.dayMod);
        }
        */
      

        /*
        int IComparer.Comparer(object obj1, object obj2)
        {
            Signal s1 = (Signal)obj1;
            Signal s2 = (Signal)obj2;
            if (s1.dayMod > s2.dayMod)
                return 1;
            if (s1.dayMod < s2.dayMod)
                return 2;
            else
                return 0;
        }    
        */
        /*
        public int CompareTo(object obj)
        {
            if (obj is Signal)
            {
                Temperature temp = (Temperature)obj;

                return m_value.CompareTo(temp.m_value);
            }

            throw new ArgumentException("object is not a Temperature");
        }
        */
    }
}
