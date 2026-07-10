using System;
using System.Collections.Generic;
using SVGImporter;
using UnityEngine;

public class NavigationAct : MonoBehaviour
{
	public SVGAsset moonAsset;

	public SVGAsset stationAsset;

	public SVGAsset[] planetAssets;

	public SpaceUI scSpaceUI;

	public static NavigationAct diff;

	public List<NavPoint> navigation = new List<NavPoint>();

	private int _length;

	public List<Backgrounds> placeToLand = new List<Backgrounds>
	{
		Backgrounds.moon,
		Backgrounds.planet,
		Backgrounds.station
	};

	public List<NavPoint> goals = new List<NavPoint>();

	public int goaltoremove = -1;

	public bool hasLanded;

	private string lastpoint = "";

	private void Awake()
	{
		diff = this;
	}

	private void Start()
	{
		GameAct gameAct = GameAct.diff;
		gameAct.OnDataChange = (Action<Variables, int>)Delegate.Combine(gameAct.OnDataChange, new Action<Variables, int>(ChangeDistance));
		GameAct gameAct2 = GameAct.diff;
		gameAct2.OnGameInit = (Action<GameSave>)Delegate.Combine(gameAct2.OnGameInit, new Action<GameSave>(InitNav));
	}

	private void InitNav(GameSave save)
	{
		if (save != null)
		{
			_length = save.datavar.Find((DataVariable it) => it.var == Variables.length)?.val ?? 10;
			navigation = new List<NavPoint>(save.navigation);
			goals = new List<NavPoint>(save.goals);
			goaltoremove = save.goaltoremove;
		}
	}

	public SVGAsset GetIconPlace(Backgrounds type, string name = "")
	{
		return type switch
		{
			Backgrounds.defaut => null, 
			Backgrounds.moon => moonAsset, 
			Backgrounds.station => stationAsset, 
			_ => planetAssets[Util.GetInt(name + "icon", 0, planetAssets.Length)], 
		};
	}

	private void ChangeDistance(Variables var, int value)
	{
		if (var == Variables.length && _length != value)
		{
			scSpaceUI.UpdateDistance(value);
			CheckDestinationsAndAdd(_length - value);
			_length = value;
		}
	}

	public NavPoint GetArrivalPoint()
	{
		if (hasLanded)
		{
			return null;
		}
		List<NavPoint> list = navigation.FindAll((NavPoint it) => it.distance == 0);
		if (list.Count == 0)
		{
			return null;
		}
		NavPoint navPoint = list[list.Count - 1];
		RemovePoint(navPoint);
		lastpoint = navPoint.name;
		return navPoint;
	}

	private void CheckDestinationsAndAdd(int add = 0)
	{
		if (GameAct.diff.card.name == "_arrival")
		{
			return;
		}
		int num = 1000;
		NavPoint navPoint = null;
		foreach (NavPoint item in navigation)
		{
			item.distance = Mathf.Clamp(item.distance + add, 0, 1000);
			if (num > item.distance)
			{
				num = item.distance;
				navPoint = item;
			}
		}
		if (navigation.Count == 0 && GameAct.diff.GetInt(Variables.nb_fame) > 10 && !GameAct.diff.GetBool("unknown"))
		{
			SetRouteValue(Util.Rand().ToString(), 0, full: false);
			AddPoint();
		}
		else if (navPoint != null)
		{
			SVGAsset iconPlace = GetIconPlace(navPoint.type, navPoint.name);
			if (iconPlace == null)
			{
				scSpaceUI.UpdateDestinationSignal(navPoint);
			}
			else
			{
				scSpaceUI.UpdateDestination(iconPlace, navPoint);
			}
		}
	}

	public void Deactivate()
	{
		scSpaceUI.gameObject.SetActive(value: false);
	}

	public void Activate()
	{
		scSpaceUI.gameObject.SetActive(value: true);
	}

	private void RemovePoint(NavPoint point)
	{
		navigation.Remove(point);
	}

	public void AddPoint(string name, int distance, Backgrounds type)
	{
		navigation.Add(new NavPoint(name, type, distance, -1, automatic: true));
		navigation.Sort((NavPoint p1, NavPoint p2) => p1.distance.CompareTo(p2.distance));
		CheckDestinationsAndAdd();
	}

	public void AddPoint()
	{
		int distance = Util.RandInt(10, 20);
		Backgrounds type = PickAPlace(Util.Rand());
		AddPoint(type, distance);
	}

	private Backgrounds PickAPlace(float r)
	{
		if (!(r > 0.1f))
		{
			if (!(r > 0.05f))
			{
				return Backgrounds.station;
			}
			return Backgrounds.moon;
		}
		return Backgrounds.planet;
	}

	public void AddPoint(Backgrounds type, int distance)
	{
		AddPoint(SpeechAct.diff.GenerateName(Util.Rand().ToString()), distance, type);
	}

	public void AddPoint(Card card, int distance)
	{
		string n = (string.IsNullOrEmpty(card.place_name) ? SpeechAct.diff.GenerateName(Util.Rand().ToString()) : card.place_name);
		navigation = new List<NavPoint>
		{
			new NavPoint(n, card.place, distance, card.id, automatic: false)
		};
		CheckDestinationsAndAdd();
	}

	public bool HasGoal(int cid)
	{
		bool num = goals.Find((NavPoint it) => it.cid == cid) != null;
		if (num)
		{
			goaltoremove = cid;
		}
		return num;
	}

	public bool HasGoal(string name)
	{
		return goals.Find((NavPoint it) => it.name == name) != null;
	}

	public bool HasFacility(string name, string facility)
	{
		if (facility == "shipyard")
		{
			return true;
		}
		float num = Util.GetFloat(name + facility);
		return facility switch
		{
			"shop" => num > 0.6f, 
			"concert" => num > 0.7f, 
			"bar" => num > 0.5f, 
			_ => false, 
		};
	}

	private string SetGoal(NavPoint point)
	{
		int d = GameAct.diff.GetDistance();
		int cid = GameAct.diff.card.id;
		NavPoint navPoint = goals.Find((NavPoint it) => it.cid == cid);
		if (navPoint != null)
		{
			navPoint.distance = d;
			navPoint.name = point.name;
		}
		else
		{
			goals.Add(new NavPoint(point.name, point.type, d, cid, automatic: true));
		}
		goals.RemoveAll((NavPoint it) => it.distance < d - 200);
		return point.name;
	}

	private string SetGoal(string goal)
	{
		return SetGoal(new NavPoint(goal, Backgrounds.defaut, 0, 0, automatic: true));
	}

	public string GetSetGoal(bool nextonly = false, Backgrounds type = Backgrounds.none)
	{
		string nameBack = BackgroundAct.diff.nameBack;
		if (nextonly)
		{
			if (navigation.Count > 0 && !hasLanded && !string.IsNullOrEmpty(lastpoint))
			{
				return navigation[0].name;
			}
			return lastpoint;
		}
		if (!hasLanded && navigation.Count > 0)
		{
			List<NavPoint> list = ((type == Backgrounds.none) ? navigation : navigation.FindAll((NavPoint it) => it.type == type));
			if (list.Count == 0)
			{
				if (type == Backgrounds.none)
				{
					AddPoint(Backgrounds.planet, Util.RandInt(5, 15));
				}
				else
				{
					AddPoint(type, Util.RandInt(5, 15));
				}
				return SetGoal(navigation[0]);
			}
			NavPoint goal = list[Util.RandInt(0, list.Count)];
			return SetGoal(goal);
		}
		List<int> list2 = new List<int>();
		for (int num = 0; num < 64; num++)
		{
			list2.Add(num);
		}
		list2.Shuffle();
		foreach (int item in list2)
		{
			Backgrounds spotType = GetSpotType(nameBack, item);
			if ((type == Backgrounds.none && placeToLand.Contains(spotType)) || (type != Backgrounds.none && spotType == type))
			{
				return SetGoal(GetSpotName(nameBack, item));
			}
		}
		return SetGoal(GetSpotName(nameBack, Util.RandInt(0, 10)));
	}

	public Backgrounds GetSpotType(string key, int id)
	{
		if (Util.GetFloat("random" + key + id) > 0.3f && id % 7 != 2)
		{
			return Backgrounds.none;
		}
		if (Util.GetFloat("rand" + key + id) > 0.7f)
		{
			return Backgrounds.defaut;
		}
		return PickAPlace(Util.GetFloat("type" + key + (float)id * 1.10537f));
	}

	public string GetSpotName(string key, int id)
	{
		return SpeechAct.diff.GenerateName("name" + key + Mathf.PingPong((float)id * 134.14537f, 1.001f));
	}

	public void SetRouteValue(string quadrantname, int stops, bool full = true)
	{
		int max = (full ? 10 : 2);
		int min = ((!full) ? (-2) : 0);
		int num = (full ? (stops * 2 + Mathf.RoundToInt((float)Util.GetInt(quadrantname + "popul", min, max) * 0.2f)) : Util.GetInt(quadrantname + "popul", min, max));
		int num2 = Util.GetInt(quadrantname + "rich", min, max);
		int num3 = Util.GetInt(quadrantname + "pirate", min, max);
		int num4 = Util.GetInt(quadrantname + "conglo", min, max);
		int num5 = Util.GetInt(quadrantname + "fringe", min, max);
		if (full)
		{
			GameAct.diff.SetInt("populous", num);
			GameAct.diff.SetInt("rich", num2);
			GameAct.diff.SetInt("piracy", num3);
			GameAct.diff.SetInt("conglo", num4);
			GameAct.diff.SetInt("fringe", num5);
		}
		else
		{
			GameAct.diff.AddInt("populous", num, 0, 10);
			GameAct.diff.AddInt("rich", num2, 0, 10);
			GameAct.diff.AddInt("piracy", num3, 0, 10);
			GameAct.diff.AddInt("conglo", num4, 0, 10);
			GameAct.diff.AddInt("fringe", num5, 0, 10);
		}
		int num6 = GameAct.diff.GetInt("conglo_friend");
		int num7 = GameAct.diff.GetInt("pirate_friend");
		int num8 = GameAct.diff.GetInt("fringe_friend");
		int num9 = 7 - num7 - num8 - num6;
		int val = Mathf.Clamp((num3 * (2 - num7) + num2 * 2 + num + num5 * (1 - num8) + num4 * (1 - num6)) / num9, 0, 10);
		Util.Write("route> pop" + num + " rich" + num2 + " pir" + num3 + " conglo" + num4 + " fringe" + num5 + " danger" + val + " conglo_friend" + num6 + " pirate_friend" + num7 + " fringe_friend" + num8);
		GameAct.diff.SetInt("danger", val);
	}

	public void SetRoute(List<MapSpot> spots)
	{
		scSpaceUI.UnsetDestination();
		ResetSpaceLockturn(GameAct.diff.GetCards(), GameAct.diff.GetInt(Variables.turns));
		ResetSpaceLockturn(GameAct.diff.GetHiddenCards(), GameAct.diff.GetInt(Variables.turns));
		hasLanded = false;
		navigation = new List<NavPoint>();
		int num = 0;
		foreach (MapSpot spot in spots)
		{
			num += spot.distance;
			AddPoint(spot.name, num, spot.type);
		}
		CheckDestinationsAndAdd();
	}

	public void ShowUI(Backgrounds place, string placename)
	{
		NavPoint navPoint = navigation.Find((NavPoint it) => it.name == placename);
		if (navPoint != null)
		{
			navPoint = navigation.Find((NavPoint it) => it.distance == 0 && it.type == place);
		}
		if (navPoint != null)
		{
			RemovePoint(navPoint);
		}
		if (placeToLand.Contains(place))
		{
			lastpoint = "";
			Land(place);
		}
		else
		{
			Launch();
		}
	}

	private void Land(Backgrounds place)
	{
		hasLanded = true;
		scSpaceUI.ShowPlace(place);
		GameAct.diff.SetInt(Variables.overall, 0);
		GameAct.diff.SetBool("simulator", boo: false);
		JukeBox.diff.PlayMusic();
	}

	private void Launch()
	{
		if (goaltoremove > -1)
		{
			goals.RemoveAll((NavPoint it) => it.cid == goaltoremove);
			goaltoremove = -1;
		}
		hasLanded = false;
		scSpaceUI.ShowShip();
	}

	private void ResetSpaceLockturn(List<Card> cards, int nextturn)
	{
		foreach (Card card in cards)
		{
			if (card.lockturn == -10)
			{
				card.nextturn = nextturn;
			}
		}
	}

	public void SetLastPointName(string name)
	{
		lastpoint = name;
	}

	public string GetLastPointName()
	{
		if (navigation.Count == 0 || !string.IsNullOrEmpty(lastpoint))
		{
			return lastpoint;
		}
		return navigation[0].name;
	}

	public int GetNextPointDistance()
	{
		if (navigation.Count == 0)
		{
			return 100;
		}
		return navigation[0].distance;
	}

	public string GetName(Backgrounds type)
	{
		NavPoint navPoint = navigation.Find((NavPoint it) => it.type == type && type != Backgrounds.defaut);
		if (navPoint != null)
		{
			return navPoint.name;
		}
		return "Sector " + SpeechAct.diff.GenerateName(Util.Rand().ToString());
	}

	public bool TestNav(Backgrounds place, int distance, Conditions operation)
	{
		if (distance == 0 && operation == Conditions.equal)
		{
			if (hasLanded)
			{
				return BackgroundAct.diff.PlaceMatch(place);
			}
			return false;
		}
		return operation switch
		{
			Conditions.equal => navigation.Find((NavPoint it) => it.type == place && it.distance == distance), 
			Conditions.below => navigation.Find((NavPoint it) => it.type == place && it.distance < distance), 
			Conditions.above => navigation.Find((NavPoint it) => it.type == place && it.distance > distance), 
			_ => null, 
		} != null;
	}

	public bool TestNav(int cid, int distance, Conditions operation)
	{
		return operation switch
		{
			Conditions.equal => navigation.Find((NavPoint it) => it.cid == cid && it.distance == distance), 
			Conditions.below => navigation.Find((NavPoint it) => it.cid == cid && it.distance < distance), 
			Conditions.above => navigation.Find((NavPoint it) => it.cid == cid && it.distance > distance), 
			_ => null, 
		} != null;
	}
}
