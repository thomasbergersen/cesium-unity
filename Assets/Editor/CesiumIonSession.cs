using Reinterop;
using UnityEngine;

namespace CesiumForUnity
{
    [ReinteropNativeImplementation("CesiumForUnityNative::CesiumIonSessionImpl", "CesiumIonSessionImpl.h")]
    public partial class CesiumIonSession
    {
        public static CesiumIonSession currentSession = null!;

        public delegate void GUIUpdateDelegate();

        public static event GUIUpdateDelegate OnConnectionUpdated;
        public static event GUIUpdateDelegate OnAssetsUpdated;
        public static event GUIUpdateDelegate OnProfileUpdated;
        public static event GUIUpdateDelegate OnTokensUpdated;

        public partial bool IsConnected();
        public partial bool IsConnecting();
        public partial bool IsResuming();
        
        public partial bool IsProfileLoaded();
        public partial bool IsLoadingProfile();

        public partial bool IsAssetListLoaded();
        public partial bool IsLoadingAssetList();

        public partial bool IsTokenListLoaded();
        public partial bool IsLoadingTokenList();

        public partial void Connect();
        public partial void Resume();
        public partial void Disconnect();

        public partial string GetProfileUsername();
        public partial string GetAuthorizeUrl();

        public void TriggerConnectionUpdate()
        {
            Debug.Log("Connection update");
            if (OnConnectionUpdated != null)
            {
                OnConnectionUpdated();
            }
        }

        public void TriggerAssetsUpdate()
        {
            if (OnAssetsUpdated != null)
            {
                OnAssetsUpdated();
            }
        }

        public void TriggerProfileUpdate()
        {
            if (OnProfileUpdated != null)
            {
                OnProfileUpdated();
            }
        }

        public void TriggerTokensUpdate()
        {
            if (OnTokensUpdated != null)
            {
                OnTokensUpdated();
            }
        }
    }
}