#include "Cesium3DTilesetImpl.h"

#include "CameraManager.h"
#include "UnityPrepareRendererResources.h"
#include "UnityTilesetExternals.h"

#include <Cesium3DTilesSelection/IonRasterOverlay.h>
#include <Cesium3DTilesSelection/Tileset.h>

#include <DotNet/CesiumForUnity/Cesium3DTileset.h>
#include <DotNet/CesiumForUnity/Cesium3DTilesetLoadFailureDetails.h>
#include <DotNet/CesiumForUnity/Cesium3DTilesetLoadType.h>
#include <DotNet/CesiumForUnity/CesiumDataSource.h>
#include <DotNet/CesiumForUnity/CesiumGeoreference.h>
#include <DotNet/CesiumForUnity/CesiumRasterOverlay.h>
#include <DotNet/CesiumForUnity/CesiumRuntimeSettings.h>
#include <DotNet/System/Action.h>
#include <DotNet/System/Array1.h>
#include <DotNet/System/Object.h>
#include <DotNet/System/String.h>
#include <DotNet/UnityEngine/Application.h>
#include <DotNet/UnityEngine/GameObject.h>
#include <DotNet/UnityEngine/Time.h>

#if UNITY_EDITOR
#include <DotNet/UnityEditor/CallbackFunction.h>
#include <DotNet/UnityEditor/EditorApplication.h>
#endif

using namespace Cesium3DTilesSelection;
using namespace DotNet;

namespace CesiumForUnityNative {

Cesium3DTilesetImpl::Cesium3DTilesetImpl(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset)
    : _pTileset(),
      _lastUpdateResult(),
#if UNITY_EDITOR
      _updateInEditorCallback(nullptr),
#endif
      _georeference(nullptr),
      _georeferenceChangedCallback(nullptr),
      _creditSystem(nullptr),
      _destroyTilesetOnNextUpdate(false) {
}

Cesium3DTilesetImpl::~Cesium3DTilesetImpl() {}

void Cesium3DTilesetImpl::JustBeforeDelete(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  this->OnDisable(tileset);
}

void Cesium3DTilesetImpl::Start(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {}

void Cesium3DTilesetImpl::Update(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  assert(tileset.enabled());

  // If "Suspend Update" is true, return early.
  if (tileset.suspendUpdate()) {
    return;
  }

  if (this->_destroyTilesetOnNextUpdate) {
    this->_destroyTilesetOnNextUpdate = false;
    this->DestroyTileset(tileset);
  }

#if UNITY_EDITOR
  // If "Update In Editor" is false, return early.
  if (UnityEngine::Application::isEditor() &&
      !UnityEditor::EditorApplication::isPlaying() &&
      !tileset.updateInEditor()) {
    return;
  }
#endif

  if (!this->_pTileset) {
    this->LoadTileset(tileset);
    if (!this->_pTileset)
      return;
  }

  std::vector<ViewState> viewStates =
      CameraManager::getAllCameras(tileset.gameObject());

  const ViewUpdateResult& updateResult = this->_pTileset->updateView(
      viewStates,
      DotNet::UnityEngine::Time::deltaTime());
  this->updateLastViewUpdateResultState(tileset, updateResult);

  for (auto pTile : updateResult.tilesFadingOut) {
    if (pTile->getState() != TileLoadState::Done) {
      continue;
    }

    const Cesium3DTilesSelection::TileContent& content = pTile->getContent();
    const Cesium3DTilesSelection::TileRenderContent* pRenderContent =
        content.getRenderContent();
    if (pRenderContent) {
      CesiumGltfGameObject* pCesiumGameObject =
          static_cast<CesiumGltfGameObject*>(
              pRenderContent->getRenderResources());
      if (pCesiumGameObject && pCesiumGameObject->pGameObject) {
        pCesiumGameObject->pGameObject->SetActive(false);
      }
    }
  }

  for (auto pTile : updateResult.tilesToRenderThisFrame) {
    if (pTile->getState() != TileLoadState::Done) {
      continue;
    }

    const Cesium3DTilesSelection::TileContent& content = pTile->getContent();
    const Cesium3DTilesSelection::TileRenderContent* pRenderContent =
        content.getRenderContent();
    if (pRenderContent) {
      CesiumGltfGameObject* pCesiumGameObject =
          static_cast<CesiumGltfGameObject*>(
              pRenderContent->getRenderResources());
      if (pCesiumGameObject && pCesiumGameObject->pGameObject) {
        pCesiumGameObject->pGameObject->SetActive(true);
      }
    }
  }
}

void Cesium3DTilesetImpl::OnValidate(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  // Check if "Suspend Update" was the modified value.
  if (tileset.suspendUpdate() != tileset.previousSuspendUpdate()) {
    // If so, don't destroy the tileset.
    tileset.previousSuspendUpdate(tileset.suspendUpdate());
  } else {
    // Otherwise, destroy the tileset so it can be recreated with new settings.
    // Unity does not allow us to destroy GameObjects and MonoBehaviours in this
    // callback, so instead it is marked to happen later.
    this->_destroyTilesetOnNextUpdate = true;
  }
}

void Cesium3DTilesetImpl::OnEnable(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
#if UNITY_EDITOR
  // In the Editor, Update will only be called when something
  // changes. We need to call it continuously to allow tiles to
  // load.
  if (UnityEngine::Application::isEditor() &&
      !UnityEditor::EditorApplication::isPlaying()) {
    this->_updateInEditorCallback = UnityEditor::CallbackFunction(
        [this, tileset]() { this->Update(tileset); });
    UnityEditor::EditorApplication::update(
        UnityEditor::EditorApplication::update() +
        this->_updateInEditorCallback);
  }
#endif
}

void Cesium3DTilesetImpl::OnDisable(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
#if UNITY_EDITOR
  if (this->_updateInEditorCallback != nullptr) {
    UnityEditor::EditorApplication::update(
        UnityEditor::EditorApplication::update() -
        this->_updateInEditorCallback);
    this->_updateInEditorCallback = nullptr;
  }
#endif

  if (this->_georeferenceChangedCallback != nullptr) {
    this->_georeference.remove_changed(this->_georeferenceChangedCallback);
  }

  this->_georeferenceChangedCallback = nullptr;
  this->_georeference = nullptr;
  this->_creditSystem = nullptr;

  this->DestroyTileset(tileset);
}

void Cesium3DTilesetImpl::RecreateTileset(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  this->DestroyTileset(tileset);
}

Tileset* Cesium3DTilesetImpl::getTileset() { return this->_pTileset.get(); }

const Tileset* Cesium3DTilesetImpl::getTileset() const {
  return this->_pTileset.get();
}
const DotNet::CesiumForUnity::CesiumCreditSystem&
Cesium3DTilesetImpl::getCreditSystem() const {
  return this->_creditSystem;
}

void Cesium3DTilesetImpl::setCreditSystem(
    const DotNet::CesiumForUnity::CesiumCreditSystem& creditSystem) {
  this->_creditSystem = creditSystem;
}

void Cesium3DTilesetImpl::updateLastViewUpdateResultState(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset,
    const Cesium3DTilesSelection::ViewUpdateResult& currentResult) {
  if (!tileset.logSelectionStats())
    return;

  const ViewUpdateResult& previousResult = this->_lastUpdateResult;
  if (currentResult.tilesToRenderThisFrame.size() !=
          previousResult.tilesToRenderThisFrame.size() ||
      currentResult.tilesLoadingLowPriority !=
          previousResult.tilesLoadingLowPriority ||
      currentResult.tilesLoadingMediumPriority !=
          previousResult.tilesLoadingMediumPriority ||
      currentResult.tilesLoadingHighPriority !=
          previousResult.tilesLoadingHighPriority ||
      currentResult.tilesVisited != previousResult.tilesVisited ||
      currentResult.culledTilesVisited != previousResult.culledTilesVisited ||
      currentResult.tilesCulled != previousResult.tilesCulled ||
      currentResult.maxDepthVisited != previousResult.maxDepthVisited) {
    SPDLOG_LOGGER_INFO(
        this->_pTileset->getExternals().pLogger,
        "{0}: Visited {1}, Culled Visited {2}, Rendered {3}, Culled {4}, Max "
        "Depth Visited {5}, Loading-Low {6}, Loading-Medium {7}, Loading-High "
        "{8}",
        tileset.gameObject().name().ToStlString(),
        currentResult.tilesVisited,
        currentResult.culledTilesVisited,
        currentResult.tilesToRenderThisFrame.size(),
        currentResult.tilesCulled,
        currentResult.maxDepthVisited,
        currentResult.tilesLoadingLowPriority,
        currentResult.tilesLoadingMediumPriority,
        currentResult.tilesLoadingHighPriority);
  }

  this->_lastUpdateResult = currentResult;
}

void Cesium3DTilesetImpl::DestroyTileset(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  // Remove any existing raster overlays
  System::Array1<CesiumForUnity::CesiumRasterOverlay> overlays =
      tileset.gameObject().GetComponents<CesiumForUnity::CesiumRasterOverlay>();
  for (int32_t i = 0, len = overlays.Length(); i < len; ++i) {
    CesiumForUnity::CesiumRasterOverlay overlay = overlays[i];
    overlay.RemoveFromTileset();
  }

  this->_pTileset.reset();
}

void Cesium3DTilesetImpl::LoadTileset(
    const DotNet::CesiumForUnity::Cesium3DTileset& tileset) {
  TilesetOptions options{};
  options.maximumScreenSpaceError = tileset.maximumScreenSpaceError();
  options.preloadAncestors = tileset.preloadAncestors();
  options.preloadSiblings = tileset.preloadSiblings();
  options.forbidHoles = tileset.forbidHoles();
  options.maximumSimultaneousTileLoads = tileset.maximumSimultaneousTileLoads();
  options.maximumCachedBytes = tileset.maximumCachedBytes();
  options.loadingDescendantLimit = tileset.loadingDescendantLimit();
  options.enableFrustumCulling = tileset.enableFrustumCulling();
  options.enableFogCulling = tileset.enableFogCulling();
  options.enforceCulledScreenSpaceError =
      tileset.enforceCulledScreenSpaceError();
  options.culledScreenSpaceError = tileset.culledScreenSpaceError();
  options.enableLodTransitionPeriod = tileset.useLodTransitions();
  options.lodTransitionLength = tileset.lodTransitionLength();
  options.showCreditsOnScreen = tileset.showCreditsOnScreen();
  options.loadErrorCallback =
      [tileset](const TilesetLoadFailureDetails& details) {
        int typeValue = (int)details.type;
        CesiumForUnity::Cesium3DTilesetLoadFailureDetails unityDetails(
            tileset,
            CesiumForUnity::Cesium3DTilesetLoadType(typeValue),
            details.statusCode,
            System::String(details.message));

        CesiumForUnity::Cesium3DTileset::BroadcastCesium3DTilesetLoadFailure(
            unityDetails);
      };

  TilesetContentOptions contentOptions{};
  contentOptions.generateMissingNormalsSmooth = tileset.generateSmoothNormals();

  options.contentOptions = contentOptions;

  this->_lastUpdateResult = ViewUpdateResult();

  if (tileset.tilesetSource() ==
      CesiumForUnity::CesiumDataSource::FromCesiumIon) {
    System::String ionAccessToken = tileset.ionAccessToken();
    if (System::String::IsNullOrEmpty(ionAccessToken)) {
      ionAccessToken =
          CesiumForUnity::CesiumRuntimeSettings::defaultIonAccessToken();
    }

    this->_pTileset = std::make_unique<Tileset>(
        createTilesetExternals(tileset),
        tileset.ionAssetID(),
        ionAccessToken.ToStlString(),
        options);
  } else {
    this->_pTileset = std::make_unique<Tileset>(
        createTilesetExternals(tileset),
        tileset.url().ToStlString(),
        options);
  }

  // Add any overlay components
  System::Array1<CesiumForUnity::CesiumRasterOverlay> overlays =
      tileset.gameObject().GetComponents<CesiumForUnity::CesiumRasterOverlay>();
  for (int32_t i = 0, len = overlays.Length(); i < len; ++i) {
    CesiumForUnity::CesiumRasterOverlay overlay = overlays[i];
    overlay.AddToTileset();
  }
}

} // namespace CesiumForUnityNative
