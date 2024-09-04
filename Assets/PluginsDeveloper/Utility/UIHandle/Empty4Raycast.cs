using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    public class Empty4Raycast : MaskableGraphic
    {
        protected Empty4Raycast()
        {
            useLegacyMeshGeneration = false;
        }

        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            toFill.Clear();
        }

#if UNITY_EDITOR
		[UnityEditor.MenuItem("GameObject/UI/Procedure/Empty4Raycast", false, 2)]
		public static void AddProceduralImage()
		{
			GameObject o = new GameObject();
			o.AddComponent<Empty4Raycast>();
			o.layer = LayerMask.NameToLayer("UI");
			o.name = "Empty4Raycast";
			if (UnityEditor.Selection.activeGameObject != null && UnityEditor.Selection.activeGameObject.GetComponentInParent<Canvas>() != null)
			{
				o.transform.SetParent(UnityEditor.Selection.activeGameObject.transform, false);
				UnityEditor.Selection.activeGameObject = o;
			}
			else
			{
				if (GameObject.FindObjectOfType<Canvas>() == null)
				{
					UnityEditor.EditorApplication.ExecuteMenuItem("GameObject/UI/Canvas");
				}
				Canvas c = GameObject.FindObjectOfType<Canvas>();

				//Set Texcoord shader channels for canvas
				c.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1 | AdditionalCanvasShaderChannels.TexCoord2 | AdditionalCanvasShaderChannels.TexCoord3;

				o.transform.SetParent(c.transform, false);
				UnityEditor.Selection.activeGameObject = o;
			}
		}
#endif
	}
}

