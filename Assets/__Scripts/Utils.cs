using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum BoundsTest {
	center,			// is the center of the gameObject on screen?
	onScreen,		// are bounds entirely on screen
	offScreen 		//are bounds entirely off screen
}

public class Utils : MonoBehaviour
{
	// create bounds that expand to hold the two bounds passed in
	public static Bounds BoundsUnion( Bounds b0, Bounds b1) {
		if (b0.size == Vector3.zero && b1.size != Vector3.zero) {
			return (b1);
		} else if (b0.size != Vector3.zero && b1.size == Vector3.zero) {
			return (b0);
		} else if (b0.size == Vector3.zero && b1.size == Vector3.zero) {
			return (b0);
		}

		// else combine them
		b0.Encapsulate (b1.min);
		b0.Encapsulate (b1.max);
		return (b0);
	}


	public static Bounds CombineBoundsOfChildren(GameObject go) 
	{
		Bounds b = new Bounds (Vector3.zero, Vector3.zero);
		if (go.GetComponent<Renderer>() != null) {
			b = BoundsUnion(b, go.GetComponent<Renderer>().bounds);
		}

		if (go.GetComponent<Collider>() != null) {
			b = BoundsUnion(b, go.GetComponent<Collider>().bounds);
		}

		foreach (Transform t in go.transform) {
				b = BoundsUnion(b, CombineBoundsOfChildren(t.gameObject));
		}

		return (b);

	}


//PROPERTY
	static public Bounds camBounds {
		get {
			if (_camBounds.size == Vector3.zero) {
				SetCameraBounds();
			}
			return (_camBounds);
		} // end of get
	}

    static private Bounds _camBounds;

    //function used by camBound property and also may be called directly

    public static void SetCameraBounds(Camera cam=null) {
		// use the main camera as default if none passed in.
		if (cam == null)
			cam = Camera.main;
			
		// assuming camera is orthographic and does not have any rotation applied to it	
		// get top left and bottomRight
		
			Vector3 topLeft = new Vector3(0,0,0);
			Vector3 bottomRight = new Vector3(Screen.width, Screen.height, 0);
			
			Vector3 boundTLN = cam.ScreenToWorldPoint(topLeft);
			Vector3	boundBRF = cam.ScreenToWorldPoint(bottomRight);	
			
			boundTLN.z = cam.nearClipPlane;
			boundBRF.z = cam.farClipPlane;
			
			Vector3 center = (boundTLN + boundBRF) /2f;
			
			_camBounds = new Bounds(center, Vector3.zero);
			_camBounds.Encapsulate(boundTLN);
			_camBounds.Encapsulate(boundBRF);
	} // end setCameraBounds
	
	// checks to see whether the bounds bnd are within the camBounds
	public static Vector3 ScreenBoundsCheck (Bounds bnd, BoundsTest test = BoundsTest.center) {
		return (BoundsInBoundsCheck( camBounds, bnd, test));
	}
	
	// Checks to see if bounds lilb are within Bounds bigB
	public static Vector3 BoundsInBoundsCheck (Bounds bigB, Bounds lilB, BoundsTest test = BoundsTest.onScreen) {
		// behavior needs to be different depending on the test selected
		
		Vector3 pos = lilB.center;		// use center for measurement
		Vector3 off = Vector3.zero;		// offset is 0,0,0 to start
		
		switch (test) {
			// what is offset to move center of lilB back inside bigB
			case BoundsTest.center:
			// trivial case - we are already inside
			if (bigB.Contains(pos)) {
				return (Vector3.zero);   //no need to move
			}
			
			//otherwise adjust x,y,z components as needed
			if(pos.x > bigB.max.x) {
				off.x = pos.x - bigB.max.x;
			} else if (pos.x < bigB.min.x) {
				off.x = pos.x - bigB.min.x;
			}
			
			if(pos.y > bigB.max.y) {
				off.y = pos.y - bigB.max.y;
			} else if (pos.y < bigB.min.y) {
				off.y = pos.y - bigB.min.y;
			}
			
			if(pos.z > bigB.max.z) {
				off.z = pos.z - bigB.max.z;
			} else if (pos.z < bigB.min.z) {
				off.z = pos.z - bigB.min.z;
			}
				
			return (off);

			//-------------------------
			// what is the offset to keep ALL of lilB inside bigB
		case BoundsTest.onScreen:
			// trivial case - we are already inside
			if (bigB.Contains(lilB.max) && bigB.Contains(lilB.min)) {
				return (Vector3.zero);   //no need to move
			}
			
			if(lilB.max.x > bigB.max.x) {
				off.x = lilB.max.x - bigB.max.x;
			} else if (lilB.min.x < bigB.min.x) {
				off.x = lilB.min.x - bigB.min.x;
			}
			
			if(lilB.max.y > bigB.max.y) {
				off.y = lilB.max.y - bigB.max.y;
			} else if (lilB.min.y < bigB.min.y) {
				off.y = lilB.min.y - bigB.min.y;
			}
			
			if(lilB.max.z > bigB.max.z) {
				off.z = lilB.max.z - bigB.max.z;
			} else if (lilB.min.z < bigB.min.z) {
				off.z = lilB.min.z - bigB.min.z;
			}
			
			return (off);
			
			//-------------------------
			// what is the offset to keep ALL of lilB outside of bigB					
		case BoundsTest.offScreen:
			bool cMin = bigB.Contains(lilB.min);
			bool cMax = bigB.Contains(lilB.max);
			
			if (cMin || cMax) {
				return (Vector3.zero);
			}
			
			
			if(lilB.min.x > bigB.max.x) {
				off.x = lilB.min.x - bigB.max.x;
			} else if (lilB.max.x < bigB.min.x) {
				off.x = lilB.max.x - bigB.min.x;
			}
			
			if(lilB.min.y > bigB.max.y) {
				off.y = lilB.min.y - bigB.max.y;
			} else if (lilB.max.y < bigB.min.y) {
				off.y = lilB.max.y - bigB.min.y;
			}
			
			if(lilB.min.z > bigB.max.z) {
				off.z = lilB.min.z - bigB.max.z;
			} else if (lilB.max.z < bigB.min.z) {
				off.z = lilB.max.z - bigB.min.z;
			}
		
			return (off);
		} // end switch BoundsTest
		
		return (Vector3.zero);  // if we get here something went wrong
	
	} // end BoundsInBoundsCheck
	
	public static GameObject FindTaggedParent(GameObject go)
    {
        if(go.tag != "Untagged")
        {
            return (go);
        }
        if(go.transform.parent == null)
        {
            return (null);
        }
        return (FindTaggedParent(go.transform.parent.gameObject));
    }

    public static GameObject FindTaggedParent(Transform t)
    {
        return (FindTaggedParent(t.gameObject));
    }

    static public Material[] GetAllMaterials(GameObject go)
    {
        List<Material> mats = new List<Material>();
        if(go.GetComponent<Renderer>() != null)
        {
            mats.Add(go.GetComponent<Renderer>().material);
        }
        foreach(Transform t in go.transform)
        {
            mats.AddRange(GetAllMaterials(t.gameObject));
        }
        return (mats.ToArray());
    }
	
    static public Vector3 Lerp(Vector3 vFrom, Vector3 vTo, float u)
    {
        Vector3 res = (1 - u) * vFrom + u * vTo;
        return (res);
    }

    static public Vector2 Lerp(Vector2 vFrom, Vector2 vTo, float u)
    {
        Vector2 res = (1 - u) * vFrom + u * vTo;
        return (res);
    }

    static public float Lerp(float vFrom, float vTo, float u)
    {
        float res = (1 - u) * vFrom + u * vTo;
        return (res);
    }

    static public Vector3 Bezier(float u, List<Vector3> vList)
    {
        if(vList.Count == 1)
        {
            return (vList[0]);
        }

        List<Vector3> vListR = vList.GetRange(1, vList.Count - 1);
        vList.RemoveAt(vList.Count - 1);
        Vector3 res = Lerp(Bezier(u, vList), Bezier(u, vListR), u);
        return (res);
    }

    static public Vector3 Bezier(float u, params Vector3[] vecs)
    {
        return (Bezier(u, new List<Vector3>(vecs)));
    }

    static public Vector2 Bezier(float u, List<Vector2> vList)
    {
        if(vList.Count == 1)
        {
            return (vList[0]);
        }

        List<Vector2> vListR = vList.GetRange(1, vList.Count - 1);
        vList.RemoveAt(vList.Count - 1);
        Vector2 res = Lerp(Bezier(u, vList), Bezier(u, vListR), u);
        return (res);
    }

    static public Vector2 Bezier(float u, params Vector2[] vecs)
    {
        return (Bezier(u, new List<Vector2>(vecs)));
    }
}

[System.Serializable]
public class EasingCachedCurve {
    public List<string> curves = new List<string>();
    public List<float> mods = new List<float>();
}

public class Easing {
    static public string Liner = ",Linear|";
    static public string In = ",In|";
    static public string Out = ",Out";
    static public string InOut = ",InOut|";
    static public string Sin = ",Sin|";
    static public string SinIn = ",SinIn|";
    static public string SinOut = ",SinOut|";

    static public Dictionary<string, EasingCachedCurve> cache;

    static public float Ease(float u, params string[] curveParams)
    {
        if(cache == null)
        {
            cache = new Dictionary<string, EasingCachedCurve>();
        }

        float u2 = u;
        foreach(string curve in curveParams)
        {
            if (!cache.ContainsKey(curve))
            {
                EaseParse(curve);
            }
            u2 = EaseP(u2, cache[curve]);
        }
        return (u2);
    }

    static private void EaseParse(string curveIn)
    {
        EasingCachedCurve ecc = new EasingCachedCurve();
        string[] curves = curveIn.Split(',');
        foreach(string curve in curves)
        {
            if (curve == "") continue;
            string[] curveA = curve.Split('|');
            ecc.curves.Add(curveA[0]);
            if(curveA.Length == 1 || curveA[1] == "")
            {
                ecc.mods.Add(float.NaN);
            } else
            {
                float parseRes;
                if(float.TryParse(curveA[1], out parseRes))
                {
                    ecc.mods.Add(parseRes);
                }
                else
                {
                    ecc.mods.Add(float.NaN);
                }
            }
        }
        cache.Add(curveIn, ecc);
    }

    static public float Ease(float u, string curve, float mod)
    {
        return (EaseP(u, curve, mod));
    }

    static private float EaseP(float u, EasingCachedCurve ec)
    {
        float u2 = u;
        for(int i=0; i < ec.curves.Count; i++)
        {
            u2 = EaseP(u2, ec.curves[i], ec.mods[i]);
        }
        return (u2);
    }

    static private float EaseP(float u, string curve, float mod)
    {
        float u2 = u;

        switch (curve)
        {
            case "In":
                if (float.IsNaN(mod)) mod = 2;
                u2 = Mathf.Pow(u, mod);
                break;

            case "Out":
                if (float.IsNaN(mod)) mod = 2;
                u2 = 1 - Mathf.Pow(1 - u, mod);
                break;

            case "InOut":
                if (float.IsNaN(mod)) mod = 2;
                if(u <= 0.5f)
                {
                    u2 = 0.5f * Mathf.Pow(u * 2, mod);
                }
                else
                {
                    u2 = 0.5f + 0.5f * (1 - Mathf.Pow(1 - (2 * (u - 0.5f)), mod));
                }
                break;

            case "Sin":
                if (float.IsNaN(mod)) mod = 2;
                u2 = u + mod * Mathf.Sin(2 * Mathf.PI * u);
                break;

            case "SinIn":
                u2 = 1 - Mathf.Cos(u * Mathf.PI * 0.5f);
                break;

            case "SinOut":
                u2 = Mathf.Sin(u * Mathf.PI * 0.5f);
                break;

            case "Linear":
            default:
                break;
        }
        return (u2);
    }
}


