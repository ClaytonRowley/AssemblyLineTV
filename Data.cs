using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductionLineTV
{
    public class Date
    {
        public string name;
        public List<Sequence> sequences;
        public int ordCount;
        public bool green;

        public Date()
        {
            ordCount = 0;
            name = "";
            sequences = new List<Sequence>();
            green = true;
        }
    }

    public class Sequence
    {
        public string name;
        public List<Route> routes;
        public int ordCount;
        public bool green;

        public Sequence()
        {
            ordCount = 0;
            name = "";
            routes = new List<Route>();
            green = true;
        }
    }

    public class Route
    {
        public string name;
        public List<OrderNo> orderNos;
        public int ordCount;
        public bool green;

        public Route()
        {
            ordCount = 0;
            name = "";
            orderNos = new List<OrderNo>();
            green = true;
        }
    }

    public class OrderNo
    {
        public string name;
        public int qty;
        public int qc;
        public int qeol;
        public bool green;

        public OrderNo()
        {
            name = "";
            qty = 0;
            qc = 0;
            qeol = 0;
            green = true;
        }
    }

    class Target
    {
        public int eight;
        public int nine;
        public int ten;
        public int eleven;
        public int twelve;
        public int one;
        public int two;
        public int three;
        public int four;
    }
}
