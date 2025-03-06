using UnityEngine;

public class ToggleComponentsOnClick : MonoBehaviour
{
    // 需要依次激活的组件
    public GameObject firstComponent;   // 第一个组件
    public GameObject secondComponent;  // 第二个组件

    // 记录当前状态
    private int clickState = 0;

    void OnMouseDown()
    {
        // 根据当前状态决定激活哪个组件
        switch (clickState)
        {
            case 0:
                ActivateComponent(firstComponent);
                clickState = 1; // 更新状态
                break;
            case 1:
                ActivateComponent(secondComponent);
                clickState = 0; // 更新状态（循环回到初始）
                break;
        }
    }

    // 激活指定组件
    private void ActivateComponent(GameObject component)
    {
        if (component != null)
        {
            // 确保目标组件有效并激活
            component.SetActive(true);
            Debug.Log(component.name + " 已激活！");
        }
        else
        {
            Debug.LogWarning("目标组件为空，无法激活！");
        }
    }
}

