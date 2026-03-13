using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Example : MonoBehaviour
{
    [SerializeField] private Button spendBtn;

    private void Start()
    {
        spendBtn.onClick.AddListener(OnSpend);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (EnergySystem.Instance.TrySpend(1))
            {
                SceneManager.LoadScene(1);
            }
        }
    }

    private void OnSpend()
    {
        EnergySystem.Instance.TrySpend(10);
    }
}
