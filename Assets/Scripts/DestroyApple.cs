using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyApple : MonoBehaviour
{
    // Start is called before the first frame update
    SpriteRenderer sprite;
    SpriteRenderer shadow;
    float alphaChange = 0.1f;
    void Start()
    {
        sprite = GetComponent<SpriteRenderer>();
        shadow = transform.GetChild(0).GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    public void destroy(){
        StartCoroutine(fadeApple());
    }

    IEnumerator fadeApple(){
        while (sprite.color.a > 0){
            shadow.color = new Color(0,0,0, sprite.color.a - alphaChange);
            sprite.color = new Color(1,1,1, sprite.color.a - alphaChange);
            yield return new WaitForSeconds(0.3f);
        }
        sprite.color = new Color(1,1,1,0);
        shadow.color = new Color(0,0,0,0);
    }
}
