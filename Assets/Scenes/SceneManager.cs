using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
	public const string SceneStart = "Start";

	public const string SceneEnd = "End";

	public const string SceneLanding = "Landing";

	public const string SceneCredits = "Credits";

	public string ClassName = "SceneManager";

	public static SceneInfo scenenInfo;

	public static TeleportInfo currentTeleport;

	private static RoleManager _roleManager;

	public static SceneManager Instance;

	public static bool loading;

	public static bool showLogo;

	public static bool loadingFromSave;

	private Color col = new Color(0f, 0f, 0f, 0f);

	public static RoleManager RoleMan
	{
		get
		{
			return SceneManager._roleManager;
		}
		set
		{
			SceneManager._roleManager = value;
		}
	}

	private void Awake()
	{
		SceneManager.Instance = this;
		SceneManager.loading = false;
		if (!ResourcePath.IS_PUBLISH || !MapControl.LoadAsync)
		{
			this.GameStart();
		}
	}

	public void GameStart()
	{
		if (!Application.isEditor)
		{
			PublishLog.Open();
		}
		SaveLoadManager.Delete(SaveLoadManager.tagSL.Del_Auto);
		this.Init();
		if (Application.loadedLevelName != "Start" && Application.loadedLevelName != "Landing" && Application.loadedLevelName != "End" && Application.loadedLevelName != "Credits")
		{
			this.InitScene();
			GameTime.Init();
		}
		if (Application.loadedLevelName == "Landing")
		{
			this.PlayGameBgSound();
		}
		if (Application.loadedLevelName != "Start" && Application.loadedLevelName != "Landing" && Application.loadedLevelName != "End" && Application.loadedLevelName != "Credits")
		{
			GUIControl.MovieClose();
			this.EnterScene();
			Singleton<CResourcesStaticManager>.GetInstance();
			EZGUIManager._BindRunTimeObj.AddRunGUIEx();
		}
		if (Application.loadedLevelName == "Landing")
		{
			EZGUIManager._BindRunTimeObj.AddLandUI();
			this.ReStartGame(true);
		}
		else
		{
			EZGUIManager._BindRunTimeObj.RemoveLandUI();
		}
		if (Application.loadedLevelName == "End" || Application.loadedLevelName == "Credits")
		{
			this.ReStartGame(false);
		}
		Main.Instance.DelayGC(20f);
	}

	private void ReStartGame(bool show)
	{
		UICamera.Instance.uiCamera.gameObject.SetActive(show);
		KeyManager.hotKeyEnabled = show;
	}

	public static void LoadLevel(string name, bool isShow, bool bLoadSave, bool fromSave)
	{
		if (SceneManager.loading)
		{
			UnityEngine.Debug.LogWarning(DU.Warning(new object[]
			{
				"Load twice"
			}));
			return;
		}
		SceneManager.loading = true;
		LoadMachine.isLoadCompleted = false;
		LandPlane.m_bAddInput = true;
		Singleton<HpCautionEffect>.GetInstance().SetRender(false, false);
		if (isShow && name != "Landing")
		{
			Singleton<EZGUIManager>.GetInstance().GetGUI<LoadingMain>().Show();
		}
		if (bLoadSave)
		{
			SDManager.SetRoleDate();
			SDManager.AddCurSceneDate();
		}
		SceneManager.loadingFromSave = fromSave;
		Main.Instance.StartCoroutine(SceneManager.WaitToLoad(name));
	}

	[DebuggerHidden]
	private static IEnumerator WaitToLoad(string name)
	{
		SceneManager.<WaitToLoad>c__Iterator8 <WaitToLoad>c__Iterator = new SceneManager.<WaitToLoad>c__Iterator8();
		<WaitToLoad>c__Iterator.name = name;
		<WaitToLoad>c__Iterator.<$>name = name;
		return <WaitToLoad>c__Iterator;
	}

	public static void LoadLevel(int levIdx, bool isShow, bool bLoadSave)
	{
		if (SceneManager.loading)
		{
			UnityEngine.Debug.LogWarning(DU.Warning(new object[]
			{
				"Load twice"
			}));
			return;
		}
		SceneManager.loading = true;
		LoadMachine.isLoadCompleted = false;
		Singleton<HpCautionEffect>.GetInstance().SetRender(false, false);
		LandPlane.m_bAddInput = true;
		if (HelpManager._instance != null)
		{
			HelpManager._instance.HelpExitEx();
		}
		if (isShow && levIdx != 1)
		{
			Singleton<EZGUIManager>.GetInstance().GetGUI<LoadingMain>().Show();
		}
		if (bLoadSave)
		{
			SDManager.SetRoleDate();
			SDManager.AddCurSceneDate();
		}
		SceneManager.loadingFromSave = !bLoadSave;
		Main.Instance.StartCoroutine(SceneManager.WaitToLoad(levIdx));
	}

	[DebuggerHidden]
	private static IEnumerator WaitToLoad(int levelindex)
	{
		SceneManager.<WaitToLoad>c__Iterator9 <WaitToLoad>c__Iterator = new SceneManager.<WaitToLoad>c__Iterator9();
		<WaitToLoad>c__Iterator.levelindex = levelindex;
		<WaitToLoad>c__Iterator.<$>levelindex = levelindex;
		return <WaitToLoad>c__Iterator;
	}

	public static void ResetSaveData()
	{
		SDManager.SDSave.Reset();
		SDManager.SDSave = new SaveData();
		DynamicData.SetDate(SDManager.SDSave.SaveDateGame.MoiveInfoList);
	}

	public static void LoadLanding()
	{
		SceneManager.LoadLevel("Landing", false, false, false);
		if (Singleton<EZGUIManager>.GetInstance().GetGUI("LandPlane") != null)
		{
			Singleton<EZGUIManager>.GetInstance().GetGUI<LandPlane>().StartCoroutine(SceneManager.ShowLand());
		}
	}

	[DebuggerHidden]
	private static IEnumerator ShowLand()
	{
		return new SceneManager.<ShowLand>c__IteratorA();
	}

	public static void LoadPrevLevel()
	{
		int num = Application.loadedLevel;
		if (num > 1)
		{
			num--;
			SceneManager.currentTeleport = null;
			SceneManager.LoadLevel(num, true, true);
		}
	}

	public static void LoadNextLevel()
	{
		int num = Application.loadedLevel;
		if (num < Application.levelCount - 1)
		{
			num++;
			SceneManager.currentTeleport = null;
			SceneManager.LoadLevel(num, true, true);
		}
	}

	private void Init()
	{
		this.GetSceneInfo();
		Main.InitMain();
		UICamera.InitUICamera();
		SystemSetting.initialize();
		Singleton<ActorManager>.GetInstance().Clear();
		if (Application.loadedLevelName == "Start")
		{
			if (!SceneManager.showLogo)
			{
				SceneManager.showLogo = true;
				Main.Instance.StartCoroutine(this.LogoMovie());
			}
			else
			{
				Main.Instance.StartCoroutine(this.GameStartCG());
			}
		}
		if (Application.loadedLevelName == "End")
		{
			Main.Instance.StartCoroutine(this.GameOverCG());
		}
		if (Application.loadedLevelName == "Credits")
		{
			Main.Instance.StartCoroutine(this.Credits());
		}
	}

	[DebuggerHidden]
	private IEnumerator LogoMovie()
	{
		return new SceneManager.<LogoMovie>c__IteratorB();
	}

	[DebuggerHidden]
	private IEnumerator GameStartCG()
	{
		return new SceneManager.<GameStartCG>c__IteratorC();
	}

	[DebuggerHidden]
	private IEnumerator GameOverCG()
	{
		return new SceneManager.<GameOverCG>c__IteratorD();
	}

	[DebuggerHidden]
	private IEnumerator Credits()
	{
		return new SceneManager.<Credits>c__IteratorE();
	}

	private void GetSceneInfo()
	{
		SceneManager.scenenInfo = GameData.Instance.cacheData.getSceneInfo(Application.loadedLevelName);
		if (SceneManager.scenenInfo == null)
		{
			UnityEngine.Debug.LogError("not find scene info:" + Application.loadedLevelName);
		}
	}

	private void InitScene()
	{
		if (SceneManager.currentTeleport != null)
		{
			PlayerInfo.PLAYER_POSITION = new Vector3(SceneManager.currentTeleport.playerX, SceneManager.currentTeleport.playerY, SceneManager.currentTeleport.playerZ);
			PlayerInfo.PLAYER_ROTATION = new Vector3(0f, SceneManager.currentTeleport.rotY, 0f);
		}
		else
		{
			PlayerInfo.PLAYER_POSITION = new Vector3(SceneManager.scenenInfo.posX, SceneManager.scenenInfo.posY, SceneManager.scenenInfo.posZ);
			PlayerInfo.PLAYER_ROTATION = new Vector3(0f, SceneManager.scenenInfo.rotY, 0f);
		}
		PlayerInfo.PLAYER_REVIVE_POSITION = new Vector3(SceneManager.scenenInfo.revive_x, SceneManager.scenenInfo.revive_y, SceneManager.scenenInfo.revive_z);
		PlayerInfo.PLAYER_REVIVE_ROTATION = new Vector3(0f, SceneManager.scenenInfo.revive_rot_y, 0f);
		this.AddComponentMaker();
		if (!MapControl.LoadAsync || !ResourcePath.IS_PUBLISH)
		{
			this.DoScriptMoudle();
		}
	}

	public void DoScriptMoudle()
	{
		if (!GUIBind.binded || Player.Instance == null)
		{
			TimeOutManager.SetTimeOut(Main.Instance.transform, 0.02f, new Callback(this.DoScriptMoudle));
		}
		else
		{
			TimeOutManager.SetTimeOut(Main.Instance.transform, 0.5f, new Callback(this.HideLoading));
			if (SceneManager.scenenInfo != null && SceneManager.scenenInfo.scriptModId != -1)
			{
				GameData.Instance.ScrMan.Exec(27, SceneManager.scenenInfo.scriptModId);
			}
			TimeOutManager.SetTimeOut(Main.Instance.transform, 1f, new Callback(Singleton<EZGUIManager>.GetInstance().GetGUI<LoadingMain>().Hide));
		}
	}

	private void HideLoading()
	{
		if (Singleton<EZGUIManager>.GetInstance().GetGUI<LoadingMain>())
		{
			Singleton<EZGUIManager>.GetInstance().GetGUI<LoadingMain>().Hide();
		}
		else
		{
			UnityEngine.Debug.LogWarning("not find LoadingMain!");
		}
	}

	private void AddComponentMaker()
	{
		SceneManager._roleManager = base.gameObject.AddComponent<RoleManager>();
		Singleton<ActorManager>.GetInstance().MainCamera = base.gameObject;
		GameObject gameObject = new GameObject("MovieManager");
		MovieManager movieMag = gameObject.AddComponent<MovieManager>();
		MovieManager.MovieMag = movieMag;
		Singleton<ActorManager>.GetInstance().MainCamera = base.gameObject;
		GameObject gameObject2 = new GameObject("SkillManager");
		CSkillManager.Instance = gameObject2.AddComponent<CSkillManager>();
	}

	private void EnterScene()
	{
		this.PlayGameBgSound();
		TeleportManager.InitTeleport(SceneManager.scenenInfo.id);
		SystemSetting.SceneSetting();
	}

	private void PlayGameBgSound()
	{
		SingletonMono<MusicManager>.GetInstance().PlayMusic(SceneManager.scenenInfo.bgSoundId, 0f, 1f, 0f);
		SingletonMono<AudioManager>.GetInstance().PauseAll(true);
	}

	private void OnGUI()
	{
		if (Application.loadedLevelName == null || Application.loadedLevelName != "Landing" || LoadingMain.loadingIsShow)
		{
			return;
		}
		if (string.IsNullOrEmpty(Main.Version))
		{
			return;
		}
		Rect position = new Rect(10f, (float)Screen.height - 30f, 200f, 20f);
		GUI.backgroundColor = this.col;
		GUI.Label(position, "Version : " + Main.Version);
	}
}
