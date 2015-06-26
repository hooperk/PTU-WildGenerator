using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Generator
{
    public class Stats : ViewModel
    {
        private int bhp, batk, bdef, bsatk, bsdef, bspd;// base stats
        public int INHP
        {
            get { return bhp; }
            set
            {
                bhp = value;
                ApplyNature();
                OnPropertyChanged("INHP");
            }
        }
        public int INATK
        {
            get { return batk; ; }
            set
            {
                batk = value;
                ApplyNature();
                OnPropertyChanged("INATK");
            }
        }
        public int INDEF
        {
            get { return bdef; }
            set
            {
                bdef = value;
                ApplyNature();
                OnPropertyChanged("INDEF");
            }
        }
        public int INSPATK
        {
            get { return bsatk; }
            set
            {
                bsatk = value;
                ApplyNature();
                OnPropertyChanged("INSPATK");
            }
        }
        public int INSPDEF
        {
            get { return bsdef; }
            set
            {
                bsdef = value;
                ApplyNature();
                OnPropertyChanged("INSPDEF");
            }
        }
        public int INSPD
        {
            get { return bspd; }
            set
            {
                bspd = value;
                ApplyNature();
                OnPropertyChanged("INSPD");
            }
        }
        public int ahp { get; set; }
        public int aatk { get; set; }
        public int adef {get; set;}
        public int asatk {get; set;}
        public int asdef {get; set;}
        public int aspd { get; set; }//after nature
        public int hp {get; set;}
        public int atk {get; set;}
        public int def {get; set;} 
        public int spatk {get; set;}
        public int spdef {get; set;}
        public int spd { get; set; }//boost number
        public int ohp {get; set;}
        public int oatk {get; set;}
        public int odef {get; set;}
        public int osatk {get; set;}
        public int osdef {get; set;}
        public int ospd { get; set; }//output values
        private int uproll, downroll;//(uproll-1)*6+downroll for nature
        private Random rand = new Random();
        private String _nature;
        public String Nature
        {
            get { return _nature; }
            set
            {
                _nature = value;
                OnPropertyChanged("Nature");
                ApplyNature();
            }
        }
        private static DoubleMap<string, int> _natures;
        public static DoubleMap<string, int> Natures
        {
            get
            {
                if (_natures == null)
                {
                    Dictionary<int, String> nature = new Dictionary<int, string>();
                    int i = 1;
                    nature.Add(i++, "Composed");
                    nature.Add(i++, "Cuddly");
                    nature.Add(i++, "Distracted");
                    nature.Add(i++, "Proud");
                    nature.Add(i++, "Decisive");
                    nature.Add(i++, "Patient");
                    nature.Add(i++, "Desperate");
                    nature.Add(i++, "Hardy");
                    nature.Add(i++, "Lonely");
                    nature.Add(i++, "Adamant");
                    nature.Add(i++, "Naughty");
                    nature.Add(i++, "Brave");
                    nature.Add(i++, "Stark");
                    nature.Add(i++, "Bold");
                    nature.Add(i++, "Docile");
                    nature.Add(i++, "Impish");
                    nature.Add(i++, "Lax");
                    nature.Add(i++, "Relaxed");
                    nature.Add(i++, "Curious");
                    nature.Add(i++, "Modest");
                    nature.Add(i++, "Mild");
                    nature.Add(i++, "Bashful");
                    nature.Add(i++, "Rash");
                    nature.Add(i++, "Quiet");
                    nature.Add(i++, "Dreamy");
                    nature.Add(i++, "Calm");
                    nature.Add(i++, "Gentle");
                    nature.Add(i++, "Careful");
                    nature.Add(i++, "Quirky");
                    nature.Add(i++, "Sassy");
                    nature.Add(i++, "Skittish");
                    nature.Add(i++, "Timid");
                    nature.Add(i++, "Hasty");
                    nature.Add(i++, "Jolly");
                    nature.Add(i++, "Naive");
                    nature.Add(i++, "Serious");
                    _natures = new DoubleMap<string, int>(nature);
                }
                return _natures;
            }
        }
        public int Level { get; set; }

        public Stats(){
            Nature = Natures.Keys.First();
        }

        public void SetNature()
        {
            downroll = rand.Next(1, 7);
            uproll = rand.Next(1, 7);
            Nature = Natures[(uproll - 1) * 6 + downroll];
        }

        public void SetRolls()
        {
            downroll = (Natures[Nature] - 1) / 6;//6 = 0, 7=1, 12 = 1,13=2, etc.
            uproll = Natures[Nature] % 6;
            if (uproll == 0) uproll = 6;
        }

        public void ApplyNature()
        {
            ahp = Math.Max(bhp + (uproll == 1 ? 1 : 0) - (downroll == 1 ? 1 : 0),1);
            aatk = Math.Max(batk + (uproll == 2 ? 2 : 0) - (downroll == 2 ? 2 : 0),1);
            adef = Math.Max(bdef + (uproll == 3 ? 2 : 0) - (downroll == 3 ? 2 : 0),1);
            asatk = Math.Max(bsatk + (uproll == 4 ? 2 : 0) - (downroll == 4 ? 2 : 0),1);
            asdef = Math.Max(bsdef + (uproll == 5 ? 2 : 0) - (downroll == 5 ? 2 : 0),1);
            aspd = Math.Max(bspd + (uproll == 6 ? 2 : 0) - (downroll == 6 ? 2 : 0),1);
            OnPropertiesChanged("ahp", "aatk", "adef", "asatk", "asdef", "aspd");
        }

        public void Calculate()
        {
            int[] stats = new int[] { ahp, aatk, adef, asatk, asdef, aspd };
            int[] ranks = new int[6];
            List<int> previous = new List<int>();
            int removed = 0;
            int rank = 0;
            while (removed < 6)
            {
                rank++;
                int highest = stats.Except(previous).Max();
                for (int i = 0; i < 6; i++)
                {
                    if (stats[i] == highest)
                    {
                        ranks[i] = rank;
                        removed++;
                    }
                }
                previous.Add(highest);
            }
            int assign = 10 + Level;
            while (assign > 0)
            {
                bool ok = true;
                int roll = rand.Next(0, 6);
                if (ranks[roll] > 1)
                {//if not highest stat
                    for (int i = 0; i < 6; i++)
                    {
                        if (ranks[i] < ranks[roll] && stats[i] == stats[roll] + 1)
                            ok = false;
                    }
                }
                if (ok)
                {
                    stats[roll]++;
                    assign--;
                }
            }
            ohp = stats[0]; hp = ohp - ahp;
            oatk = stats[1]; atk = oatk - aatk;
            odef = stats[2]; def = odef - adef;
            osatk = stats[3]; spatk = osatk - asatk;
            osdef = stats[4]; spdef = osdef - asdef;
            ospd = stats[5]; spd = ospd - aspd;
            OnPropertiesChanged("ohp", "hp", "oatk", "atk", "odef", "def", "osatk", "spatk", "osdef", "spdef", "ospd", "spd");
        }

        private ICommand _randomnature;
        public ICommand RandomNature
        {
            get
            {
                if (_randomnature == null)
                {
                    _randomnature = new RelayCommand(o => SetNature());
                }
                return _randomnature;
            }
        }

        private ICommand _statup;
        public ICommand StatUp
        {
            get
            {
                if (_statup == null)
                {
                    _statup = new RelayCommand(o => Calculate());
                }
                return _statup;
            }
        }
    }
}
