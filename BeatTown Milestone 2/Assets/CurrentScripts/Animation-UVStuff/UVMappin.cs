using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]

public class UVMappin : MonoBehaviour

{
	[SerializeField]
	public Texture2D _texture; // This is the motion we're going to be mapping on.
	public Texture2D sourceTexture;
	public Texture2D baseMask;

	public StateMachine sm;

	/// <summary>
	/// Maps sourceTexture onto this motion, using baseMask. Use the optional sourceOffset argument to define the starting point on the sourceTexture.
	/// Returns a list of sprites with sourceTexture mapped on each frame of this motion.
	/// </summary>
	[SerializeField]
	public List<Sprite> MapOntoTexture(Texture2D sourceTexture, Texture2D baseMask, Vector2Int? sourceOffset = null)
	{
		var l = new List<Sprite>(); // This is the return value containing the output spritesheet.

		// Create mapping
		// Each color of the map will correspond to a color from the source texture

		Dictionary<Color, Color> mapping = new Dictionary<Color, Color>();

		// baseMask is assumed to have an aspect ratio of 1:1
		for (int x = 0; x < baseMask.width; x++)
		{
			for (int y = 0; y < baseMask.height; y++)
			{
				var maskColor = baseMask.GetPixel(x, y);

				if (mapping.ContainsKey(maskColor))
				{
					//mapping[maskColor] = Color.red; // If there's mask color collision, color them red.
				}
				else if (maskColor.a > 0f) // Let's not map fully transparent pixels
				{
					// handle sourceOffset
					var px = x;
					var py = y;

					if (sourceOffset != null)
					{
						px += sourceOffset.Value.x;
						py += sourceOffset.Value.y;
					}
					//--

					var sourceColor = sourceTexture.GetPixel(px, py);
					mapping.Add(maskColor, sourceColor); // Add a mapping from the baseMask to the sourceTexture.
				}
			}
		}

		// Construct frames, take the motion source and color it with the pixels determined by the mapping.
		for (int frame = 0; frame < _texture.width / _texture.height; frame++)
		{
			var txt = new Texture2D(_texture.height, _texture.height, TextureFormat.ARGB32, false);
			txt.filterMode = FilterMode.Point;

			for (int x = _texture.height * frame; x < _texture.height * frame + _texture.height; x++)
			{
				for (int y = 0; y < _texture.height; y++)
				{
					var motionColor = _texture.GetPixel(x, y);

					if (mapping.ContainsKey(motionColor))
					{
						txt.SetPixel(x - _texture.height * frame, y, mapping[motionColor]);
					}
					else
					{
						txt.SetPixel(x - _texture.height * frame, y, Color.clear);
					}
				}
			}

			txt.Apply();

			// Create sprite
			var spr = Sprite.Create(txt, new Rect(0, 0, _texture.height, _texture.height), Vector2.one * 0.5f);
			l.Add(spr);
		}

		return l;
	}

	List<Sprite> texturedSprites;
	int curFrame = 0;

	void Start()
	{
		
		//texturedSprites = MapOntoTexture(sourceTexture, baseMask);

		Destroy(GetComponent<Animator>());

		//GetComponent<SpriteRenderer>().sprite = texturedSprites[0];
		//StartCoroutine(Animate());

	}

	IEnumerator Animate()
	{
		while (true)
		{
			GetComponent<SpriteRenderer>().sprite = texturedSprites[curFrame++];
			if (curFrame >= texturedSprites.Count)
			{
				curFrame = 0;
			}
			yield return new WaitForSeconds(0.1f);
		}
	}
}