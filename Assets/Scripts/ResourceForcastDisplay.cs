using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResourceForcastDisplay : MonoBehaviour
{

    public TextMeshProUGUI nameText;
    public Slider currentHealthSlider;
    public Slider forecastedHealthSlider;
    public TextMeshProUGUI currentHealthText;
    public TextMeshProUGUI forecastedHealthText;
    public TextMeshProUGUI weaponDamage;
    
    public void Forecast(Unit unit, int outgoingDamage, int incomingDamage) {
        nameText.text = unit.name;
        int currentHealth = unit.health.currentValue;
        currentHealthText.text = currentHealth.ToString();

        if(incomingDamage == -1) {
            incomingDamage = 0;
        }
        int forecastedHealth = currentHealth - incomingDamage;
        if(forecastedHealth < 0) {
            forecastedHealth = 0;
        }

        forecastedHealthText.text = forecastedHealth.ToString();

        if(outgoingDamage == -1) {
            weaponDamage.text = "-";
        } else {
            weaponDamage.text = outgoingDamage.ToString();
        }

        currentHealthSlider.maxValue = unit.health.maxValue;
        currentHealthSlider.value = currentHealth;
        forecastedHealthSlider.maxValue = unit.health.maxValue;
        forecastedHealthSlider.value = forecastedHealth;

    }


}
