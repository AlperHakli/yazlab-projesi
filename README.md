# Unity Müze Soygunu Projesi

Bu proje, Kocaeli Üviversitesi Bilişim Sistemleri Mühendisliği yazlab-1 dersi için Unity oyun motoru kullanılarak geliştirilmiş, üçüncü şahıs (TPS) bir gizlilik/aksiyon oyunudur. Oyuncunun amacı, müzeye sızmak monalisa tablosunu çalmak ve öldürülmeden önce kaçış noktasına ulaşmaktır 

## 👾 Temel Özellikler

* **Üçüncü Şahıs Karakter Kontrolü:** Akıcı hareket, koşma (sprint), eğilme (crouch), zıplama ve nişan alma (zoom) mekanikleri.
* **Silah Mekanikleri:** Hem oyuncu hem de düşmanlar için anlık vuruş hitscan raycast tabanlı ateş etme sistemi. Oyuncu 'F' tuşu ile silahını çekip gizleyebilir.
* **Yapay Zeka (AI) Sistemi:** İki farklı NPC türü:
    * **Ziyaretçiler (`VisitorAI`):** `NavMesh` kullanarak müzede rastgele dolaşan sivil NPC'ler. optimizayon için oyuncudan belirli bir metre uzaklıkta hareket etmezler
    * **Güvenlik Görevlileri (`SecurityAI`):** FSM kullanan kullanıcıya alarm durumunda ateş edebilen npcler
* **Dinamik FSM Durumları:** Güvenlik görevlileri `Idle` (Boşta Durma), `Patrol` (Devriye), `Chase` (Kovalama) ve `Attack` (Saldırı) durumları arasında dinamik olarak geçiş yapar.
* **Global Alarm Sistemi:** Oyuncu ana tabloyu çaldığı an PaintingTrigger.cs sayesinde boolean bir değişken aracılığıyla global alarm tetiklenir ve *tüm* güvenlik görevlileri `Patrol` durumunu terk edip oyuncuyu aramaya başlar.
* **Evrensel Can Sistemi Health.cs scripti sayesinde hem oyuncu hem güvenlik hemde ziyaretçiler ortak bir canı paylaşır
* **Komple Oyun Döngüsü:**
    * **Kazanma Koşulu:** Tabloyu çaldıktan sonra (`isAlarmTriggered == true`) olunca kaçış noktasına ulaşmak. (`EscapeZone.cs`) scripti burada devreye giriyor
    * **Kaybetme Koşulu:** Oyuncunun canının 0'a düşmesi (`PlayerUI.cs`) scripti canı takip ederek 0 olduğu anda ekrana game over yazdırır
* **Dinamik Arayüz (UI):** Oyuncunun canını gösteren, silah çekildiğinde nişangah (crosshair) çıkaran ve oyun bittiğinde "GAME OVER" / "YOU WIN" ekranlarını gösteren bir `Canvas` sistemi.

## 🛠️ Kullanılan Teknolojiler

* **Oyun Motoru:** Unity
* **Dil:** C#
* **Ana Unity Sistemleri:**
    * **NavMesh (AI Navigation):** Tüm NPC'lerin haritada yol bulması ve gezinmesi için kullanıldı ilk seferinde update fonksiyonunda sürekli güvenlik npc leri oyuncunun yeni konumunu aramaya çalışırken yeni yazdığımız fonksiyonda update başına değilde belirli bir saniye sonra tekrardan oyuncuyu aramaya başladı böylece optimizasyon artmış oldu
    * **Animator (Mecanim):** Tüm karakterlerin (Oyuncu, Güvenlik, Ziyaretçi) animasyonlarını yönetmek için kullanıldı. Özellikle hareket için `Blend Tree` (Karışım Ağacı) ve durum geçişleri için Fine State Machine (Sonlu Durum Makinesi) mantığı uygulandı.
    * **Character Controller:** Oyuncunun fizik tabanlı olmayan, akıcı hareketi için kullanıldı (ThirdPersonController scriptinin bi kısmını ve CameraController scriptini hazır bir assetten aldım haberiniz olsun)
    * **Physics (Raycast):** Silahların "Hitscan" vuruş mekaniği ve güvenliklerin oyuncuyu "görme" (`CanSeePlayer`) mantığı için kullanıldı güvenlikten oyuncuya bir ışın atılıyor ve eğer ışın bir duvara çarpmazsa ateş edilme durumuna geçiyor
    * **UI (Canvas & TextMeshPro):** Tüm arayüz elemanları (Can yazısı, nişangah, kazanma/kaybetme ekranları) için kullanıldı.

---

### 1. Sistem şeması ve Oyun mekanikleri

* **Oyuncu Sistemi:** `ThirdPersonController.cs` script'i klavyeden (`Input`) gelen komutları alır. Bu komutlar:
    1.  `CharacterController` bileşenine `cc.Move()` komutu göndererek fiziksel hareketi sağlar.
    2.  `Animator` bileşenine `SetFloat("Speed")` veya `SetBool("isArmed")` gibi sinyaller göndererek görsel animasyonu tetikler.
* **Çatışma Sistemi (Hitscan):**
    1.  Oyuncu `Input.GetButton("Fire1")`'e yani mouse sol tık a basar.
    2.  `ThirdPersonController.cs`, `HitscanGun.cs` script'indeki `TryToShoot()` fonksiyonunu çağırır.
    3.  `HitscanGun.cs`, `Player Camera`'nın merkezinden ileriye doğru bir `Physics.Raycast` (ışın) atar.
    4.  Işın bir `Collider`'a çarparsa, `GetComponentInParent<Health>()` ile o objenin `Health.cs` script'ini arar.
    5.  Bulursa, `targetHealth.TakeDamage()` fonksiyonunu çağırır.
* **AI - Güvenlik FSM (Finite State Machine):**
    `SecurityAI.cs` script'i `Update()` içinde sürekli olarak `HandleStateTransitions()` (Durum Geçişlerini Yönet) fonksiyonunu çalıştırır. Bu fonksiyon, yapay zekanın "beynidir" ve hangi durumda (`currentState`) olacağına karar verir.

* **AI - Ziyaretçi FSM:** Çok daha basittir. `Idle` (Bekleme) ve `Walking` (Yürüme) arasında rastgele bir zamanlayıcı ile geçiş yapar.
* **Oyun Döngüsü (Kazanma/Kaybetme):**
    1.  **Kaybetme:** `Health.cs` (Player) -> `Die()` -> `OnDie.Invoke()` (Event) -> `PlayerUI.ShowGameOverScreen()` (Metodu dinler).
    2.  **Kazanma:** `PaintingTrigger.cs` (Oyuncu girer) -> `SecurityAI.isAlarmTriggered = true` (Static değişken ayarlanır) -> Oyuncu `EscapeZone.cs`'e (Trigger) girer -> `EscapeZone.cs`, `SecurityAI.isAlarmTriggered == true` olduğunu görür -> `PlayerUI.ShowWinScreen()`.

### 2. Tasarlanan Arayüz Sistemleri ("Sayfalar")

Proje, `UnityEngine.UI` (Canvas) sistemi üzerine kurulu 3 ana "sayfa" veya "ekran" durumuna sahiptir:

1.  **Oyun İçi Arayüz (In-Game HUD):**
    * **Can Göstergesi:** `PlayerUI.cs` script'i tarafından yönetilen, oyuncunun `Health.CurrentHealth` değerini anlık olarak gösteren bir `TextMeshPro` yazısı.
    * **Nişangah (Crosshair):** `PlayerUI.cs` tarafından yönetilen bir `Image`. Sadece `isArmed == true` (silahlı) durumdayken görünür.
2.  **Game Over Ekranı (`WinLossScreen_Panel`):**
    * Oyuncunun `Health.cs` script'i `Die()` fonksiyonunu çağırdığında tetiklenir.
    * `Time.timeScale = 0f` yaparak oyunu dondurur.
    * `PlayerUI.cs`, `StatusText` objesinin yazısını "GAME OVER" olarak ayarlar.
    * `Enter` tuşuna basıldığında `PlayerUI.RestartGame()` fonksiyonunu çağırarak sahneyi yeniden başlatır.
3.  **Kazanma Ekranı (`WinLossScreen_Panel`):**
    * Oyuncu, tabloyu çaldıktan sonra `EscapeZone`'a ulaştığında tetiklenir.
    * `Time.timeScale = 0f` yaparak oyunu dondurur.
    * `PlayerUI.cs`, `StatusText` objesinin yazısını "YOU WIN!" olarak ayarlar.
    * `Enter` tuşuna basıldığında `PlayerUI.RestartGame()` fonksiyonunu çağırarak sahneyi yeniden başlatır.

### 3. Literatür Taraması ve Karşılaştırma

Bu proje, "Soygun" (Heist) alt türüne sahip bir "Gizlilik/Aksiyon" (Stealth/Action) oyunudur. Bu alandaki temel literatür (örnek oyunlar) *Hitman*, *Payday* ve *Metal Gear Solid* gibi serilerdir.

* **Karşılaştırma (Hitman Serisi):** *Hitman* oyunları, `Behavior Tree` (Davranış Ağacı) veya `Goal-Oriented Action Planning` (GOAP) gibi çok daha karmaşık YZ (Yapay Zeka) mimarileri kullanır. NPC'ler sadece "görmezler", aynı zamanda "duyarlar", "şüphelenirler" ve "araştırırlar". Bizim projemiz ise hocanın da istediği gibi, daha temel bir **Sonlu Durum Makinesi (FSM)** kullanır. Bizim YZ'mızın sadece 4 net durumu (`Idle`, `Patrol`, `Chase`, `Attack`) vardır. *Hitman*'deki gibi "Şüpheli" veya "Araştır" gibi ara durumları yoktur.
* **Karşılaştırma (Payday Serisi):** *Payday*, gizlilik bozulana kadar FSM kullanır, ancak gizlilik bozulduğunda "Alarm" durumuna geçer ve bu durum (bizim projemizin aksine) *geri döndürülemez*. Bizim projemizde, eğer alarm çalmadan (`isAlarmTriggered == false`) oyuncu güvenlik tarafından görülürse (`Chase` durumu), oyuncu kaçıp saklanabilir ve güvenlik bir süre sonra `Patrol` (Devriye) durumuna geri döner. Bu, bizim FSM'mizi daha dinamik kılar.
* **Bizim Çalışmamızın Yeri:** Bu proje, klasik FSM mimarisinin modern bir prototipidir. Global durumu yönetmek için `static bool isAlarmTriggered` kullanması, küçük ölçekli projeler için etkili, ancak büyük ölçekli oyunlar için (genellikle bir `GameManager` Singleton'ı tercih edilir) daha basit bir yaklaşımdır.

### 4. Kullanılan Yazılımsal Mimariler, Yöntemler ve Teknikler

Proje boyunca birçok temel oyun geliştirme tekniği ve mimarisi uygulanmıştır:

1.  **Sonlu Durum Makinesi (Finite State Machine - FSM):** Hem `SecurityAI` hem de `VisitorAI` script'lerinin çekirdek mantığıdır. `enum AIState` (durumları tanımlamak için) ve `Update()` içindeki bir `switch` (veya `if-else`) bloğu (o anki duruma göre eylemi seçmek için) kullanılarak kodlanmıştır.
2.  **Bileşen Tabanlı Mimari (Component-Based Architecture):** Unity'nin temel felsefesidir. Bir Güvenlik Görevlisi objesi, `NavMeshAgent` (hareket), `Health` (can), `SecurityAI` (beyin) ve `Capsule Collider` (fizik) gibi birçok bağımsız bileşenin bir araya gelmesiyle oluşturulmuştur.
3.  **Olay Güdümlü Programlama (Event-Driven Programming):** `Health.cs` script'i ile `PlayerUI.cs` script'i arasındaki iletişim bu şekilde sağlanmıştır. `Health` script'i öldüğünde `OnDie` adında bir `UnityEvent`'i tetikler. `PlayerUI` script'i bu olayı *dinler* (subscribes) ve tetiklendiğinde `ShowGameOverScreen()` fonksiyonunu çalıştırır. Bu, `Health` script'inin `PlayerUI` hakkında *hiçbir şey bilmemesini* sağlar (düşük bağımlılık - low coupling).
4.  **Animasyon Yeniden Hedefleme (Animation Retargeting):** Mixamo'dan indirilen animasyonlar, `Humanoid` (İnsansı) `Rig` (İskelet) tipine ayarlanarak, hem Oyuncu (Player) hem de Güvenlik (Security) modelleri gibi farklı iskelet yapılarında sorunsuzca kullanılmıştır.
5.  **Karışım Ağaçları (Blend Trees):** "Özürlü gibi koşma" (animation stutter) sorununu çözmek için `PlayerAnimator.controller`'da kullanılmıştır. `Animator`, `run` (bool) ve `sprint` (bool) gibi iki ayrı sinyal yerine, `Speed` (Hız) adında tek bir `float` (ondalıklı sayı) sinyali alır. `Blend Tree`, bu `Speed` değerine göre `Idle` (Hız=0), `Walk` (Hız=1) ve `Run` (Hız=2) animasyonlarını akıcı bir şekilde *karıştırır*.
6.  **Performans Optimizasyonu (AI LOD):** `VisitorAI` script'i, oyuncuya olan mesafesini (`Vector3.Distance`) hesaplar. Eğer oyuncu `optimizationDistance`'tan (örn: 200m) uzaktaysa, `agent.isStopped = true` komutuyla `NavMeshAgent`'i durdurur ve `return;` komutuyla `Update()` fonksiyonunun geri kalanını çalıştırmayı atlar. Bu, uzaktaki 30 sivil NPC'nin işlemci yükünü neredeyse sıfıra indirir. `SecurityAI` için de benzer bir "karar verme zamanlayıcısı" (`decisionUpdateInterval`) kullanılmıştır.
7.  **Ters Kinematik (Inverse Kinematics - IK):** Silahın "sola bakma" veya "eğilince yukarı kayma" sorunlarını çözmek için kullanılmıştır. `Animator`'de `IK Pass` aktifleştirilmiş ve `ThirdPersonController.cs` script'ine `OnAnimatorIK()` fonksiyonu eklenmiştir. Bu fonksiyon, animasyon klibi ne derse desin, `animator.SetLookAtWeight()` ve `animator.SetLookAtPosition()` komutlarıyla karakterin üst vücudunu ve kafasını kameranın nişan aldığı hedefe (Raycast hedefi) nişan almaya zorlar.

### 5. Karşılaşılan Zorluklar ve Çözümler

Proje geliştirme süreci, özellikle `Animator` ve `NavMeshAgent` sistemlerinin entegrasyonunda zorlu geçmiştir.

* **Zorluk:** "Özürlü Gibi Koşma / Kekeleme": `Any State` (Herhangi Bir Durum) düğümü, `run == true` koşulu sağlandığı her karede animasyonu başa sarıyordu.
    * **Çözüm:** `Any State`'ten `Run`'a giden okun `Inspector`'daki `Can Transition to Self` (Kendi Kendine Geçiş Yapabilir) ayarı kapatıldı. Daha sonra bu yöntem tamamen terk edildi ve `Blend Tree` (Karışım Ağacı) yöntemine geçildi.

* **Zorluk:** "Çift Zıplama": Karakter yere indiği an (`isGrounded = true`), `Animator` `Idle` animasyonuna geçiyor, bu animasyon karakterin ayağını anlık olarak yerden kesiyor, `isGrounded = false` oluyor ve bu da `Jump` (Zıplama) animasyonunu tekrar tetikliyordu.
    * **Çözüm:** Zıplamadan çıkış (`Jump` -> `Idle`) için `Any State` kullanımı bırakıldı. Yerine `Jump` state'inden çıkan, `Has Exit Time` (Çıkış Zamanı Var) seçeneği işaretli ve `air == false` koşullu manuel bir ok (transition) oluşturuldu.

* **Zorluk:** Unity Animator Arayüzü "Saçmalığı": `Sub-State Machine` (Alt Durum Makinesi) yaratırken, bir alt durumun (`Idle`) varsayılan (default) yapılması, ana katmandaki (`Base Layer`) varsayılanı bozuyordu ("(Up) Base Layer" hatası).
    * **Çözüm:** Bu "kilitlenmeyi" çözmek için bozuk `Animator Controller` dosyası silindi ve "Yak ve Yeniden İnşa Et" yöntemi uygulandı. Yeni `controller` *doğru sırada* (Önce Parametreler -> Önce Odalar -> Sonra Odaların İçi -> En Son Ana Bağlantılar) kurularak arayüz hatası (UI bug) aşıldı.

* **Zorluk:** NPC "Oyun Başında Ölme Sesi" / "Ölü Ziyaretçi Hareket Etmiyor": `Health.cs` script'indeki `public currentHealth` değişkeninin değeri, `Inspector`'da `0` olarak "takılı kalıyordu" (Serialized Value Bug). Ayrıca `Die()` fonksiyonu AI script'ini (`SecurityAI` / `VisitorAI`) kapatıyor, bu da `Animator` parametrelerinin "donmasına" neden oluyordu.
    * **Çözüm:** `currentHealth` değişkeni `[SerializeField] private` yapıldı. `SecurityAI` ve `VisitorAI` script'leri, öldükten sonra bile `UpdateAnimation()` fonksiyonunu çalıştırmaya devam edecek (ancak diğer eylemleri durduracak) şekilde güncellendi.

### 6. Projenin Kattığı Faydalar (Kişisel Çıkarımlar)

Bu projeyi tek başıma tamamlamak, Unity motorunun çekirdek sistemleri hakkında paha biçilmez bir deneyim kazandırdı. Özellikle FSM mantığını sıfırdan kodlamak, `Animator` sisteminin `Blend Tree` ve `IK Pass` gibi gelişmiş özelliklerini kullanarak karşılaşılan "saçma" arayüz hatalarını ayıklamak (debug) ve `NavMeshAgent`'i yüksek performans için optimize etmek (düzinelerce NPC'yi yönetmek) konusunda derin bir anlayış edindim. `Health.cs` gibi modüler ve olay-güdümlü (event-driven) script'ler yazarak, kodun yeniden kullanılabilirliğini ve sistemler arası bağımlılığı (coupling) nasıl azaltacağımı öğrendim.
