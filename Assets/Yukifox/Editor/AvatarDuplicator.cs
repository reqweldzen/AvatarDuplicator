using UnityEditor;
using UnityEngine;

namespace Yukifox.Editor
{
	public class AvatarDuplicator : EditorWindow
	{
		private GameObject _sourceAvatar;
		private bool _isCopyAnimationClip = true;
		
		[MenuItem("AvatarDuplicator/Duplicator")]
		private static void ShowWindow()
		{
			var window = GetWindow<AvatarDuplicator>("AvatarDuplicator");
		}

		private void Check()
		{
			var avatarDuplicator = new AvatarDuplicatorCore(_sourceAvatar, _isCopyAnimationClip);

			if (avatarDuplicator.CheckDuplicateable())
			{
				Debug.Log("複製可能");
			}
			else
			{
				Debug.Log("複製不可");
			}
		}
		
		private void Duplicate()
		{
			var duplicator = new AvatarDuplicatorCore(_sourceAvatar, _isCopyAnimationClip);
			duplicator.DuplicateAvatar();
		}
		
		private void OnGUI()
		{
			_sourceAvatar = EditorGUILayout.ObjectField("Source Object", _sourceAvatar, typeof(GameObject), true) as GameObject;

			EditorGUILayout.Space();
			_isCopyAnimationClip = EditorGUILayout.Toggle("Copy AnimationClip", _isCopyAnimationClip);

			EditorGUI.BeginDisabledGroup(!_sourceAvatar);
			if (GUILayout.Button("Check"))
			{
				Check();
			}
			if (GUILayout.Button("Duplicate"))
			{
				Duplicate();
			}
			EditorGUI.EndDisabledGroup();
		}
	}
}