using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

using Newtonsoft.Json;
using System.Collections.ObjectModel;

namespace HawksTOA
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int API_TIMEOUT = 2000; // For somereason www.theorangealliance.org/api has a throttle on requests
        List<Team> _miLOEventTeams = new List<Team>();
        Dictionary<Team, List<Match>> _miLOMatchbyTeam = new Dictionary<Team, List<Match>>();
        Dictionary<Team, List<MatchDetail>> _miLOMatchDetailbyTeam = new Dictionary<Team, List<MatchDetail>>();
        ObservableCollection<TeamStats> _miLOTeamStats = new ObservableCollection<TeamStats>();

        public MainWindow()
        {
            InitializeComponent();
            fetchTeamsFromEvent("2122-FIM-MLOQ");//fetch based on teams at the Lake Orion event
            foreach (Team t in _miLOEventTeams)
            {
                List<Match> tmpMatches = fetchTeamMatches(t.team_key);
                _miLOMatchbyTeam[t] = tmpMatches;
                Thread.Sleep(API_TIMEOUT);
                List<MatchDetail> tmpMatchDetail = new List<MatchDetail>();
                foreach (Match m in tmpMatches)
                {
                    try
                    {
                        // reason for the try::catch is because some match detail
                        // returns an empty result '[]' TEST-TEST-CASE
                        MatchDetail md = fetchTeamMatchDetail(m.match_key);
                        if (md != null)
                        {
                            tmpMatchDetail.Add(md);
                            if(m.station /10 > 1)
                            {
                                // must be blue
                                string wlt = "W";
                                if(md.red.total_points > md.blue.total_points)
                                {
                                    wlt = "L";
                                }else if (md.red.total_points == md.blue.total_points)
                                {
                                    wlt = "T";
                                }
                                _miLOTeamStats.Add(new TeamStats(t.team_number, m.station, wlt, md.blue.auto_nav_points, md.blue.capped_points, md.blue.carousel_points, md.blue.total_points, md.red.total_points));

                            }
                            else
                            {
                                // must be red
                                string wlt = "W";
                                if (md.red.total_points < md.blue.total_points)
                                {
                                    wlt = "L";
                                }
                                else if (md.red.total_points == md.blue.total_points)
                                {
                                    wlt = "T";
                                }
                                _miLOTeamStats.Add(new TeamStats(t.team_number, m.station, wlt, md.red.auto_nav_points, md.red.capped_points, md.red.carousel_points, md.red.total_points, md.blue.total_points));
                            }
                        }

                    }
                    catch { }
                    Thread.Sleep(API_TIMEOUT);
                }
                _miLOMatchDetailbyTeam[t] = tmpMatchDetail;
            }
            // now we have all the teams competeing in the Lake Orion Event
            // now we have all the matches the teams were involved in
            // now we have all the deteils from all the matches the teams were involved in
            // next we need to parse through the data to determine statistics
            WLTGrid.ItemsSource = _miLOTeamStats;
            
        }

        public string fetchTeamsFromEvent(string eventStr)
        {

            var clientMILOEventTeams = new RestClient("https://theorangealliance.org/api/event/"+eventStr+"/teams", "1/1/1900 1:00:00 AM");
            string[] responseVal = new string[2];
            responseVal = clientMILOEventTeams.MakeRequest();
            var jsonResp = responseVal[1];

            if (jsonResp != string.Empty)
            {
                _miLOEventTeams = JsonConvert.DeserializeObject<List<Team>>(jsonResp);
            }
            return responseVal[0];
        }

        public List<Match> fetchTeamMatches(string teamKey)
        {
            var clientMILOEventTeams = new RestClient("https://theorangealliance.org/api/team/"+teamKey+"/matches/2122", "1/1/1900 1:00:00 AM");
            string[] responseVal = new string[2];
            responseVal = clientMILOEventTeams.MakeRequest();
            var jsonResp = responseVal[1];
            List<Match> returnList = new List<Match>();

            if (jsonResp != string.Empty)
            {
                returnList = JsonConvert.DeserializeObject<List<Match>>(jsonResp);
            }

            return returnList;
        }

        public MatchDetail fetchTeamMatchDetail(string matchKey)
        {
            var clientMILOEventTeams = new RestClient("https://theorangealliance.org/api/match/" + matchKey+"/details", "1/1/1900 1:00:00 AM");
            string[] responseVal = new string[2];
            responseVal = clientMILOEventTeams.MakeRequest();
            var jsonResp = responseVal[1];
            List<MatchDetail> returnList = new List<MatchDetail>();

            if (jsonResp != string.Empty && jsonResp.Length >2)
            {
                returnList = JsonConvert.DeserializeObject<List<MatchDetail>>(jsonResp);
            }

            return returnList[0];
        }

        public ObservableCollection<TeamStats> GetTeamRecord
        {
            get { return this._miLOTeamStats; }
        }
    }

    public class TeamDetail
    {
        public string team_key { get; set; }
        public string region_key { get; set; }
        public string league_key { get; set; }
        public int team_number { get; set; }
        public string team_name_short { get; set; }
        public string team_name_long { get; set; }
        public string robot_name { get; set; }
        public string last_active { get; set; }
        public string city { get; set; }
        public string state_prov { get; set; }
        public string zip_code { get; set; }
        public string country { get; set; }
        public int rookie_year { get; set; }
        public string website { get; set; }
    }

    public class Team
    {
        public string event_participant_key { get; set; }
        public string event_key { get; set; }
        public string team_key { get; set; }
        public int team_number { get; set; }
        public bool is_active { get; set; }
        public string card_status { get; set; }
        public TeamDetail team { get; set; }
    }

    public class Match
    {
        public string match_participant_key { get; set; }
        public string match_key { get; set; }
        public string team_key { get; set; }
        public int station { get; set; }
        public int station_status { get; set; }
        public int ref_status { get; set; }
        public TeamDetail team { get; set; }
    }

    public class Red
    {
        public string barcode_element_1 { get; set; }
        public string barcode_element_2 { get; set; }
        public bool carousel { get; set; }
        public string auto_navigated_1 { get; set; }
        public string auto_navigated_2 { get; set; }
        public int auto_nav_points { get; set; }
        public bool auto_bonus_1 { get; set; }
        public bool auto_bonus_2 { get; set; }
        public int auto_bonus_points { get; set; }
        public int auto_storage_freight { get; set; }
        public int auto_freight_1 { get; set; }
        public int auto_freight_2 { get; set; }
        public int auto_freight_3 { get; set; }
        public int auto_freight_points { get; set; }
        public int tele_storage_freight { get; set; }
        public int tele_freight_1 { get; set; }
        public int tele_freight_2 { get; set; }
        public int tele_freight_3 { get; set; }
        public int tele_alliance_hub_points { get; set; }
        public int tele_shared_hub_points { get; set; }
        public int tele_storage_points { get; set; }
        public int shared_freight { get; set; }
        public int end_delivered { get; set; }
        public int end_delivered_points { get; set; }
        public bool alliance_balanced { get; set; }
        public int alliance_balanced_points { get; set; }
        public bool shared_unbalanced { get; set; }
        public int shared_unbalanced_points { get; set; }
        public string end_parked_1 { get; set; }
        public string end_parked_2 { get; set; }
        public int end_parked_points { get; set; }
        public int capped { get; set; }
        public int capped_points { get; set; }
        public int carousel_points { get; set; }
        public int total_points { get; set; }
    }

    public class Blue
    {
        public string barcode_element_1 { get; set; }
        public string barcode_element_2 { get; set; }
        public bool carousel { get; set; }
        public string auto_navigated_1 { get; set; }
        public string auto_navigated_2 { get; set; }
        public int auto_nav_points { get; set; }
        public bool auto_bonus_1 { get; set; }
        public bool auto_bonus_2 { get; set; }
        public int auto_bonus_points { get; set; }
        public int auto_storage_freight { get; set; }
        public int auto_freight_1 { get; set; }
        public int auto_freight_2 { get; set; }
        public int auto_freight_3 { get; set; }
        public int auto_freight_points { get; set; }
        public int tele_storage_freight { get; set; }
        public int tele_freight_1 { get; set; }
        public int tele_freight_2 { get; set; }
        public int tele_freight_3 { get; set; }
        public int tele_alliance_hub_points { get; set; }
        public int tele_shared_hub_points { get; set; }
        public int tele_storage_points { get; set; }
        public int shared_freight { get; set; }
        public int end_delivered { get; set; }
        public int end_delivered_points { get; set; }
        public bool alliance_balanced { get; set; }
        public int alliance_balanced_points { get; set; }
        public bool shared_unbalanced { get; set; }
        public int shared_unbalanced_points { get; set; }
        public string end_parked_1 { get; set; }
        public string end_parked_2 { get; set; }
        public int end_parked_points { get; set; }
        public int capped { get; set; }
        public int capped_points { get; set; }
        public int carousel_points { get; set; }
        public int total_points { get; set; }
    }

    public class MatchDetail
    {
        public string match_detail_key { get; set; }
        public string match_key { get; set; }
        public int red_min_pen { get; set; }
        public int blue_min_pen { get; set; }
        public int red_maj_pen { get; set; }
        public int blue_maj_pen { get; set; }
        public Red red { get; set; }
        public Blue blue { get; set; }
    }

    public class TeamStats
    {
        int team_num;
        int station;
        string wlt;
        int auton_points;
        int capped_points;
        int carousel_points;
        int total_for_points;
        int total_against_points;
        public TeamStats(int team_id, int station_id, string winLossTie, int auton, int capped, int carousel,int totalFor,int totalAgainst)
        {
            team_num = team_id;
            station = station_id;
            wlt = winLossTie;
            auton_points = auton;
            capped_points = capped;
            carousel_points = carousel;
            total_for_points = totalFor;
            total_against_points = totalAgainst;

        }

        public int TeamNumber
        {
            get { return team_num; }
        }
        
        public String Station
        {
            get {

                if (station == 11)
                {
                    return "Red 1";
                }
                else if(station == 12)
                {
                    return "Red 2";
                }
                else if(station == 13)
                {
                    return "Red 3";
                }
                else if (station == 21)
                {
                    return "Blue 1";
                }
                else if (station == 22)
                {
                    return "Blue 2";
                }
                else if (station == 23)
                {
                    return "Blue 3";
                }
                else
                {
                    return "NA";
                }
            }
        }

        public String WLT { get { return wlt; } }
        public int AutonPoints { get { return auton_points; } }
        public int CappedPoints { get { return capped_points; } }
        public int CarouselPoints { get { return carousel_points; } }
        public int TotalPointsFor { get { return total_for_points; } }
        public int TotalPointsAgainst { get { return total_against_points; } }
    }
}
