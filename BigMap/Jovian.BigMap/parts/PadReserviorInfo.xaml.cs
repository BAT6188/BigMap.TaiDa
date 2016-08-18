using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Jovian.BigMap.parts
{
    /// <summary>
    /// PadReserviorInfo.xaml 的交互逻辑
    /// </summary>
    public partial class PadReserviorInfo : UserControl
    {
        private ObservableCollection<ZYZTeam> ocZYZTeam = new ObservableCollection<ZYZTeam>();
        public PadReserviorInfo()
        {
            InitializeComponent();
        }
    }

    //院桥镇人防（民防）志愿者队伍抢险抢修分队花名册
    public class ZYZTeam : INotifyPropertyChanged
    {
        private string _xm;//姓名
        private string _xb;//性别
        private string _sfzh;//身份证号
        private string _gzdw;//工作单位
        private string _zw;//职务
        private string _dt;//党团
        private string _whcd;//文化程度
        private string _lxdh;//联系电话
        private string _dh;//短号

        public ZYZTeam(string Xm, string Xb, string Sfzh, string Gzdw, string Zw, string Dt, string Whcd, string Lxdh, string Dh)
        {
            XM = Xm;
            XB = Xb;
            SFZH = Sfzh;
            GZDW = Gzdw;
            ZW = Zw;
            DT = Dt;
            WHCD = Whcd;
            LXDH = Lxdh;
            DH = Dh;
        }

        public string DH
        {
            get { return _dh; }
            set
            {
                _dh = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DH"));
            }
        }

        public string LXDH
        {
            get { return _lxdh; }
            set
            {
                _lxdh = value;
                OnPropertyChanged(new PropertyChangedEventArgs("LXDH"));
            }
        }

        public string WHCD
        {
            get { return _whcd; }
            set
            {
                _whcd = value;
                OnPropertyChanged(new PropertyChangedEventArgs("WHCD"));
            }
        }

        public string DT
        {
            get { return _dt; }
            set
            {
                _dt = value;
                OnPropertyChanged(new PropertyChangedEventArgs("DT"));
            }
        }

        public string GZDW
        {
            get { return _gzdw; }
            set
            {
                _gzdw = value;
                OnPropertyChanged(new PropertyChangedEventArgs("GZDW"));
            }
        }

        public string ZW
        {
            get { return _zw; }
            set
            {
                _zw = value;
                OnPropertyChanged(new PropertyChangedEventArgs("ZW"));
            }
        }

        public string XM
        {
            get { return _xm; }
            set
            {
                _xm = value;
                OnPropertyChanged(new PropertyChangedEventArgs("XM"));
            }
        }

        public string XB
        {
            get { return _xb; }
            set
            {
                _xb = value;
                OnPropertyChanged(new PropertyChangedEventArgs("XB"));
            }
        }

        public string SFZH
        {
            get { return _sfzh; }
            set
            {
                _sfzh = value;
                OnPropertyChanged(new PropertyChangedEventArgs("SFZH"));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, e);
        }
    }
}
