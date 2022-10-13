using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.Core;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace Yukifox.Editor
{
	public class AvatarDuplicatorCore
	{
		private readonly bool _isCopyAnimationClip;
		
		private GameObject _targetAvatar;
		private GameObject _copiedAvatar;
		private VRCAvatarDescriptor _descriptor;
		private VRCAvatarDescriptor _copiedDescriptor;
		
		public AvatarDuplicatorCore(GameObject targetAvatar, bool isCopyAnimationClip)
		{
			_targetAvatar = targetAvatar;
			_isCopyAnimationClip = isCopyAnimationClip;
			_descriptor = targetAvatar.GetComponent<VRCAvatarDescriptor>();
		}

		public bool CheckDuplicateable()
		{
			if (!CheckAvatar()) return false;

			return true;
		}

		private bool CheckAvatar()
		{
			var avatarAnimator = _targetAvatar.GetComponent<Animator>();
			if (!avatarAnimator)
			{
				Debug.Log("モデルデータではありません。");
				return false;
			}
			
			if (!_descriptor)
			{
				Debug.Log("VRCアバターではありません。");
				return false;
			}

			return true;
		}
		
		public void DuplicateAvatar()
		{
			if (!CheckDuplicateable()) return; 
			
			Setup();
		}
		
		private void Setup()
		{
			_copiedAvatar = Object.Instantiate(_targetAvatar, Vector3.zero, quaternion.identity);
			_copiedAvatar.name = _targetAvatar.name;
			_copiedDescriptor = _copiedAvatar.GetComponent<VRCAvatarDescriptor>();

			_targetAvatar.SetActive(false);

			var folderPath = "AvatarDuplicator/" + _copiedDescriptor.name;
			YFLib.CreateAssetsFolder();
			YFLib.CreateFolderRecursive(folderPath);

			// FX Layer Copy
			var fxController = _descriptor.GetController(VRCAvatarDescriptor.AnimLayerType.FX, out int index);
			if (fxController)
			{
				var newControllerPath = YFLib.GetSavePath() + "/" + folderPath + "/" + fxController.name + ".controller";
				
				AssetDatabase.DeleteAsset(newControllerPath);
				AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(fxController), newControllerPath);
				fxController = AssetDatabase.LoadAssetAtPath<AnimatorController>(newControllerPath);

				var tempLayer = _copiedDescriptor.baseAnimationLayers[index];
				tempLayer.isDefault = false;
				tempLayer.animatorController = fxController;
				_copiedDescriptor.baseAnimationLayers[index] = tempLayer;
			}
			
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			
			// AnimationClip Copy
			if (_isCopyAnimationClip)
			{
				foreach (var layer in fxController.layers)
				{
					if (YFLib.IgnoreCopyFXLayers.Contains(layer.name))
					{
						continue;
					}

					var layerName = layer.name;
					var layerPath = folderPath + "/FX/" + layerName;
					YFLib.CreateFolderRecursive(layerPath);

					foreach (var childState in layer.stateMachine.states)
					{
						var clip = childState.state.motion as AnimationClip;
						if (!clip)
						{
							// No motion.
							continue;
						}

						var newClipPath = YFLib.GetSavePath() + "/" + layerPath + "/" + clip.name + ".anim";
						AssetDatabase.DeleteAsset(newClipPath);
						AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(clip), newClipPath);
						clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(newClipPath);

						if (!clip)
						{
							EditorUtility.DisplayDialog("ERROR", "AnimationClip copy failed.", "OK");
							return;
						}

						childState.state.motion = clip;
					}
				}

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
			}

			var exMenu = _descriptor.expressionsMenu;
			var exMenuPath = AssetDatabase.GetAssetPath(exMenu);
			
			if (!string.IsNullOrEmpty(exMenuPath))
			{
				var newPath = $"{YFLib.GetSavePath()}/{folderPath}/{exMenu.name}.asset";
				AssetDatabase.DeleteAsset(newPath);
				AssetDatabase.CopyAsset(exMenuPath, newPath);
				var menu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(newPath);

				if (!menu)
				{
					EditorUtility.DisplayDialog("ERROR", "ExpressionsMenu copy failed.", "OK");
				}
				
				_copiedDescriptor.expressionsMenu = menu;
			}
			
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			
			var exParam = _descriptor.expressionParameters;
			var exParamPath = AssetDatabase.GetAssetPath(exParam);
			
			if (!string.IsNullOrEmpty(exParamPath))
			{
				var newPath = $"{YFLib.GetSavePath()}/{folderPath}/{exParam.name}.asset";
				AssetDatabase.DeleteAsset(newPath);
				AssetDatabase.CopyAsset(exParamPath, newPath);
				var param = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(newPath);

				if (!param)
				{
					EditorUtility.DisplayDialog("ERROR", "ExpressionParameters copy failed.", "OK");
				}
				
				_copiedDescriptor.expressionParameters = param;
			}
			
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			
			EditorUtility.DisplayDialog("Success", "Complete", "OK");
		}
		
		// private void SaveDuplicateAvatar(GameObject avatar)
		// {
		// 	if (!Directory.Exists(RootPath))
		// 	{
		// 		AssetDatabase.CreateFolder("Assets/YumemiDoh", "Generated");
		// 	}
		//
		// 	var avatarName = avatar.name;
		//
		// 	string rootDir = null;
		// 	if (!Directory.Exists($"{RootPath}/{avatarName}"))
		// 	{
		// 		AssetDatabase.CreateFolder(RootPath, avatarName);
		// 		rootDir = $"{RootPath}/{avatarName}";
		// 	}
		// 	else
		// 	{
		// 		for (var i = 1; i < ushort.MaxValue; i++)
		// 		{
		// 			var extraPath = $"{RootPath}/{avatarName}_{i}";
		// 			if (Directory.Exists(extraPath))
		// 				continue;
		// 			
		// 			AssetDatabase.CreateFolder(RootPath, $"{avatarName}_{i}");
		// 			rootDir = extraPath;
		// 			break;
		// 		}
		//
		// 		if (!Directory.Exists(rootDir))
		// 		{
		// 			Debug.Log("作成できる連番の制限を超えています。");
		// 			Object.DestroyImmediate(avatar);
		// 			return;
		// 		}
		// 	}
		// 	
		// 	var descriptor = avatar.GetComponent<VRCAvatarDescriptor>();
		// 	
		// 	var fx = descriptor.baseAnimationLayers[4];
		// 	var fxPath = AssetDatabase.GetAssetPath(fx.animatorController);
		// 	
		// 	var exMenu = descriptor.expressionsMenu;
		// 	var exMenuPath = AssetDatabase.GetAssetPath(exMenu);
		// 	// TODO: Copy SubMenu
		// 	var exParam = descriptor.expressionParameters;
		// 	var exParamPath = AssetDatabase.GetAssetPath(exParam);
		//
		// 	if (!string.IsNullOrEmpty(fxPath))
		// 	{
		// 		var newPath = $"{rootDir}/{fx.animatorController.name}.controller";
		// 		if (AssetDatabase.CopyAsset(fxPath, newPath))
		// 		{
		// 			var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(newPath);
		// 			descriptor.baseAnimationLayers[4].animatorController = controller;
		// 		}
		// 	}
		//
		// 	if (!string.IsNullOrEmpty(exMenuPath))
		// 	{
		// 		var newPath = $"{rootDir}/{exMenu.name}.asset";
		// 		if (AssetDatabase.CopyAsset(exMenuPath, newPath))
		// 		{
		// 			descriptor.expressionsMenu = AssetDatabase.LoadAssetAtPath<VRCExpressionsMenu>(newPath);
		// 		}
		// 	}
		// 	
		// 	if (!string.IsNullOrEmpty(exParamPath))
		// 	{
		// 		var newPath = $"{rootDir}/{exParam.name}.asset";
		// 		if (AssetDatabase.CopyAsset(exParamPath, newPath))
		// 		{
		// 			descriptor.expressionParameters = AssetDatabase.LoadAssetAtPath<VRCExpressionParameters>(newPath);
		// 		}
		// 	}
		//
		// 	var localPath = $"{rootDir}/{avatarName}.prefab";
		//
		// 	localPath = AssetDatabase.GenerateUniqueAssetPath(localPath);
		//
		// 	bool prefabSuccess;
		// 	PrefabUtility.SaveAsPrefabAssetAndConnect(avatar, localPath, InteractionMode.UserAction,
		// 		out prefabSuccess);
		//
		// 	if (prefabSuccess)
		// 	{
		// 		Debug.Log("Prefab was saved successfully");
		// 		Object.DestroyImmediate(avatar);
		// 	}
		// 	else
		// 	{
		// 		Debug.Log("Prefab failed to save.");
		// 		Object.DestroyImmediate(avatar);
		// 	}
		// }
	}
}