using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;
using Cinemachine;
using Unity.VisualScripting;

public class InventoryController : MonoBehaviour
{
   public static InventoryController instance;

   public SOInventory player_inventory;
   private PlayerInputs playerInputs;

   public Transform dropPoint;
   public float throwForce = 5f;


   private bool is_open = false;

   [SerializeField] private Transform inventory_parent;
   [SerializeField] private Transform bottom_slots_parent;

   public List<Transform> T_slots = new List<Transform>();

   public GameObject UI_prefab;


   private void Awake()
   {
      if (instance == null)
      {
         instance = this;
      }
      else if (instance != this)
      {
         Destroy(gameObject);
      }

      for (int i = 0; i < bottom_slots_parent.childCount; i++)
      {
         T_slots.Add(bottom_slots_parent.GetChild(i));
      }

      for (int i = 0; i < inventory_parent.childCount; i++)
      {
         T_slots.Add(inventory_parent.GetChild(i));
      }
   }
   private void Start()
   {
      playerInputs = GetComponent<FPSController>().playerInputs;
      playerInputs.Interaction.Tab.started += ctx => OpenInventory();
      playerInputs.Interaction.QButton.started += ctx => DropCurrentItem();
      for (int i = 0; i < player_inventory.slots.Count; i++)
      {
         if (player_inventory.slots[i].isFull)
         { DeleteItem(i); }
      }

      UpdateAllSlotUI();


   }
   public void CleanPhysicalItemsForLoop()
   {
      // Envanterdeki tüm slotları tek tek kontrol et
      for (int i = 0; i < player_inventory.slots.Count; i++)
      {
         Slot currentSlot = player_inventory.slots[i];

         // Slot dolu mu?
         if (currentSlot.isFull && currentSlot.item != null)
         {
            // --- KORUMA ŞARTLARI ---

            // 1. Şart: Eğer bu bir fotoğrafsa (texture varsa) silme, bir sonraki döngüye taşı.
            if (currentSlot.capturedTexture != null) continue;

            // 2. Şart: Eğer bu ana eşyan olan "Camera" ise silme.
            // (Item ismini SOItem içindeki 'itemName' değişkenine göre kontrol et)
            if (currentSlot.item.my_prefab.tag == "PhotoMachine") continue;

            // --- SİLME İŞLEMİ ---
            // Yukarıdaki şartlara uymayan her şeyi (fiziksel objeler) temizle
            DeleteItem(i);
         }
      }

      // Envanter temizlendikten sonra elindeki görseli (Handle) güncelle
      if (Handle.instance != null)
      {
         Handle.instance.SetHandlePrefab();
      }

      Debug.Log("<color=yellow>Envanter Temizlendi: Fiziksel eşyalar silindi, fotoğraflar korundu.</color>");
   }


   public void UpdateSlotUI(int slot_index)
   {
      if (T_slots[slot_index].childCount == 0)
      {
         Instantiate(UI_prefab, T_slots[slot_index]);
      }

      T_slots[slot_index].GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = player_inventory.slots[slot_index].amount.ToString();

      T_slots[slot_index].GetChild(0).GetComponent<RawImage>().texture = player_inventory.slots[slot_index].item_image;

      Handle.instance.SetHandlePrefab();

   }
   public void UpdateAllSlotUI()
   {
      for (int i = 0; i < T_slots.Count; i++)
      {
         Slot currentSlot = player_inventory.slots[i];
         if (currentSlot.isFull == true)
         {
            Instantiate(UI_prefab, T_slots[i]);
            T_slots[i].GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = player_inventory.slots[i].amount.ToString();

            T_slots[i].GetChild(0).GetComponent<RawImage>().texture = player_inventory.slots[i].item_image;
            Handle.instance.SetHandlePrefab();
         }
      }
   }

   public void ChangeSlotsEach(Transform slot1, Transform slot2)
   {
      int x = T_slots.IndexOf(slot1);
      int y = T_slots.IndexOf(slot2);
      //Debug.Log(y);

      Slot temple = new Slot();

      temple.isFull = player_inventory.slots[x].isFull;
      temple.amount = player_inventory.slots[x].amount;
      temple.item = player_inventory.slots[x].item;
      temple.item_image = player_inventory.slots[x].item_image;
      temple.prefab = player_inventory.slots[x].prefab;
      temple.worldInstance = player_inventory.slots[x].worldInstance;
      temple.capturedTexture = player_inventory.slots[x].capturedTexture;
      temple.storedObjectPrefab = player_inventory.slots[x].storedObjectPrefab;


      player_inventory.slots[x].isFull = player_inventory.slots[y].isFull;
      player_inventory.slots[x].amount = player_inventory.slots[y].amount;
      player_inventory.slots[x].item = player_inventory.slots[y].item;
      player_inventory.slots[x].item_image = player_inventory.slots[y].item_image;
      player_inventory.slots[x].prefab = player_inventory.slots[y].prefab;
      player_inventory.slots[x].worldInstance = player_inventory.slots[y].worldInstance;
      player_inventory.slots[x].capturedTexture = player_inventory.slots[y].capturedTexture;
      player_inventory.slots[x].storedObjectPrefab = player_inventory.slots[y].storedObjectPrefab;


      player_inventory.slots[y].isFull = temple.isFull;
      player_inventory.slots[y].amount = temple.amount;
      player_inventory.slots[y].item = temple.item;
      player_inventory.slots[y].item_image = temple.item_image;
      player_inventory.slots[y].prefab = temple.prefab;
      player_inventory.slots[y].worldInstance = temple.worldInstance;
      player_inventory.slots[y].capturedTexture = temple.capturedTexture;
      player_inventory.slots[y].storedObjectPrefab = temple.storedObjectPrefab;

      Handle.instance.SetHandlePrefab();
   }

   public void DeleteItem(int slot_index)
   {
      if (T_slots[slot_index].childCount > 0)
      { Destroy(T_slots[slot_index].transform.GetChild(0).gameObject); }

      player_inventory.slots[slot_index].isFull = false;
      player_inventory.slots[slot_index].amount = 0;
      player_inventory.slots[slot_index].item = null;
      player_inventory.slots[slot_index].item_image = null;
      player_inventory.slots[slot_index].prefab = null;
      player_inventory.slots[slot_index].worldInstance = null;
      player_inventory.slots[slot_index].capturedTexture = null;
      player_inventory.slots[slot_index].storedObjectPrefab = null;

      Handle.instance.SetHandlePrefab();
      UpdateSlotUI(slot_index);

   }

   public void DecreaseItemAmount(int slot_index)
   {
      player_inventory.slots[slot_index].amount--;
      if (player_inventory.slots[slot_index].amount == 0)
      {
         DeleteItem(slot_index);
      }
      UpdateSlotUI(slot_index);
   }
   public int FindMyIndex(Transform slot)
   {
      int x = T_slots.IndexOf(slot);
      return x;
   }

   void DropCurrentItem()
   {
      int currentIndex = Handle.instance.index;
      Slot currentSlot = player_inventory.slots[currentIndex];

      // Eğer slot boşsa hiçbir şey yapma
      if (!currentSlot.isFull) return;
      Debug.Log("Objeyi fırlat: " + currentSlot.item.name);
      // --- FOTOĞRAF KONTROLÜ ---
      // Eğer bu bir fotoğrafsa (capturedTexture doluysa), dünyada bir kopyası yoktur.
      // Direkt siliyoruz ve fonksiyondan çıkıyoruz (return).
      if (currentSlot.capturedTexture != null)
      {
         Debug.Log("Fotoğraf direkt silindi.");
         DeleteItem(currentIndex);
         return;
      }

      // --- DİĞER EŞYALAR İÇİN (Balyoz, Kamera vb.) ---
      if (currentSlot.worldInstance != null)
      {
         Debug.Log("Objeyi fırlat: " + currentSlot.item.name);
         GameObject objToDrop = currentSlot.worldInstance;

         // 1. Objeyi fırlatılacak noktaya taşı
         objToDrop.transform.position = Camera.main.transform.position;
         objToDrop.transform.rotation = dropPoint.rotation;

         // 2. Görünürlüğü ve Collider'ı tekrar aç
         objToDrop.GetComponent<MeshRenderer>().enabled = true;
         if (objToDrop.GetComponent<Collider>() != null) objToDrop.GetComponent<Collider>().enabled = true;
         foreach (var r in objToDrop.GetComponentsInChildren<Renderer>()) r.enabled = true;

         // 3. Fizik uygula
         Rigidbody rb = objToDrop.GetComponent<Rigidbody>();
         if (rb != null)
         {
            rb.isKinematic = false;
            rb.AddForce(Camera.main.transform.forward * throwForce, ForceMode.Impulse);
         }

         // 4. Envanterden sil
         DeleteItem(currentIndex);

         // 5. Elindeki görseli temizle
         Handle.instance.SetHandlePrefab();
      }
   }








   public CinemachineVirtualCamera vcam;
   #region Inputs
   public void OpenInventory()
   {
      var pov = vcam.GetCinemachineComponent<CinemachinePOV>();
      is_open = !is_open;
      inventory_parent.gameObject.SetActive(is_open);

      if (is_open)
      {
         Cursor.lockState = CursorLockMode.None;
         pov.m_HorizontalAxis.m_MaxSpeed = 0f;
         pov.m_VerticalAxis.m_MaxSpeed = 0f;
      }
      else
      {
         Cursor.lockState = CursorLockMode.Locked;
         pov.m_HorizontalAxis.m_MaxSpeed = 300f;
         pov.m_VerticalAxis.m_MaxSpeed = 300f;
      }
   }


   #endregion


}