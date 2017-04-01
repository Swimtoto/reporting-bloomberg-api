using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeutralObjects
{
    public class HistoData
    {
        private DateTime date;
        private double price;

        public HistoData(DateTime _date, double _price)
        {
            date = _date;
            price = _price;
        }

        public DateTime Date
        {
            get { return date; }
            set { }
        }

        public double Price
        {
            get { return price; }
            set { price = value; }
        }
    }
}

