using BepInEx;
using RoR2;
using UnityEngine;
using System.Collections;

[BepInPlugin("poto.warzonemod", "Warzone Style Mod v1.2", "1.2.0")]
public class WarzoneStyleMod : BaseUnityPlugin
{
    private CharacterBody playerBody;

    // Armes et munitions
    private enum WeaponSlot { Principale, Secondaire, Sniper }
    private WeaponSlot armeEquiped = WeaponSlot.Principale;
    private int chargeurPrincipale = 30;
    private int chargeurSecondaire = 30;
    private int chargeurSniper = 15;
    private int sacMunitions = 60;

    // Plaques et Auto-Réa
    private int plaques = 2;      // commence avec 2 plaques (Trempé)
    private int plaquesMax = 2;   // max plaques = 2
    private bool autoRea = false;

    // Buffs
    private float buffPrecision = 1f;
    private float buffRecul = 1f;
    private float buffPerforation = 0.2f;

    // Invisibilité
    private bool invisible = false;
    private float invisSpeedMultiplier = 1.5f;

    // HUD
    private Texture2D plaqueTexture;

    void Awake()
    {
        On.RoR2.CharacterBody.FixedUpdate += CharacterBody_FixedUpdate;
        On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;

        plaqueTexture = new Texture2D(1, 1);
        plaqueTexture.SetPixel(0, 0, Color.cyan);
        plaqueTexture.Apply();
    }

    private void CharacterBody_FixedUpdate(On.RoR2.CharacterBody.orig_FixedUpdate orig, CharacterBody self)
    {
        orig(self);
        if (!self.isPlayerControlled) return;

        playerBody = self;

        if (!invisible && self.healthComponent.combinedHealthFraction < 0.05f)
        {
            invisible = true;
            StartCoroutine(InvisibilityRoutine());
        }
    }

    private IEnumerator InvisibilityRoutine()
    {
        playerBody.modelLocator.modelTransform.gameObject.SetActive(false);
        float oldSpeed = playerBody.moveSpeed;
        playerBody.moveSpeed *= invisSpeedMultiplier;

        yield return new WaitForSeconds(1f);

        playerBody.moveSpeed = oldSpeed;
        playerBody.modelLocator.modelTransform.gameObject.SetActive(true);
        invisible = false;
    }

    private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
    {
        orig(self, damageReport);

        if (damageReport.victimMaster && damageReport.attacker && damageReport.attacker.isPlayerControlled)
        {
            plaques += 1;
            if (plaques > plaquesMax) plaques = plaquesMax;

            if (damageReport.victimMaster.isBoss && !autoRea)
            {
                autoRea = true;
                Debug.Log("Auto-Réa obtenu !");
            }
        }

        if (damageReport.victimMaster && damageReport.victimMaster.isElite)
        {
            DropMunitionsSpec(damageReport.victimMaster);
        }
    }

    private void DropMunitionsSpec(CharacterMaster elite)
    {
        int type = Random.Range(0, 3);
        switch (type)
        {
            case 0: Debug.Log("Balle explosive drop !"); break;
            case 1: Debug.Log("Balle givrante drop !"); break;
            case 2: sacMunitions += 5; Debug.Log("Munitions supplémentaires : +5"); break;
        }
    }

    void OnGUI()
    {
        if (playerBody != null)
        {
            float plaqueWidth = 50f;
            float plaqueHeight = 8f;
            float startX = 10f;
            float startY = 50f;

            for (int i = 0; i < plaquesMax; i++)
            {
                Rect rect = new Rect(startX + i * (plaqueWidth + 2), startY, plaqueWidth, plaqueHeight);
                if (i < plaques)
                    GUI.DrawTexture(rect, plaqueTexture);
                else
                    GUI.Box(rect, "");
            }

            GUI.Label(new Rect(10, 70, 250, 20), $"Munitions sac: {sacMunitions}");
            GUI.Label(new Rect(10, 90, 250, 20), $"Auto-Réa: {autoRea}");
            GUI.Label(new Rect(10, 110, 250, 20), $"Arme équipée: {armeEquiped}");
            GUI.Label(new Rect(10, 130, 250, 20), $"Buffs: Precision x{buffPrecision}, Recul x{buffRecul}, Perforation x{buffPerforation}");
        }
    }

    public void ChangerArme(WeaponSlot nouvelleArme)
    {
        armeEquiped = nouvelleArme;
        Debug.Log($"Arme changée: {armeEquiped}");
    }

    public void AppliquerBuff(float precision, float recul, float perforation)
    {
        buffPrecision = precision;
        buffRecul = recul;
        buffPerforation = perforation;
        Debug.Log($"Buffs appliqués: Precision x{buffPrecision}, Recul x{buffRecul}, Perforation x{buffPerforation}");
    }
}
