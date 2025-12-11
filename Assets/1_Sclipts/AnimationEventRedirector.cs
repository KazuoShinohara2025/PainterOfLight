using UnityEngine;

public class AnimationEventRedirector : MonoBehaviour
{
    // 親オブジェクト（ThirdPersonController）にイベントを転送する
    public void OnFootstep(AnimationEvent animationEvent)
    {
        SendMessageUpwards("OnFootstep", animationEvent, SendMessageOptions.DontRequireReceiver);
    }

    public void OnLand(AnimationEvent animationEvent)
    {
        SendMessageUpwards("OnLand", animationEvent, SendMessageOptions.DontRequireReceiver);
    }
}