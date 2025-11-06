# Unity Müze Soygunu Projesi

Bu proje, Kocaeli Üniversitesi Bilişim Sistemleri Mühendisliği yazlab-1 dersi için Unity oyun motoru kullanılarak geliştirilmiş, üçüncü şahıs (TPS) bir gizlilik/aksiyon oyunudur. Oyuncunun amacı, müzeye sızmak monalisa tablosunu çalmak ve öldürülmeden önce kaçış noktasına ulaşmaktır 

## 👾 Temel Özellikler

* **Üçüncü Şahıs Karakter Kontrolü:** Akıcı hareket, koşma (sprint), eğilme (crouch), zıplama ve nişan alma (zoom) mekanikleri.
* **Silah Mekanikleri:** Hem oyuncu hem de düşmanlar için anlık vuruş hitscan raycast tabanlı ateş etme sistemi. Oyuncu 'F' tuşu ile silahını çekip gizleyebilir.
* **Işıklandırmalar** Tamemen elle yapılmış sahne ışıklandırmaları haritanın daha doğal olması için ana harita ışık kaynağı çıkarılıp yerine harita ışıkları eklendi
* **Yapay Zeka (AI) Sistemi:** İki farklı NPC türü:
    * **Ziyaretçiler (`VisitorAI`):** `NavMesh` kullanarak müzede rastgele dolaşan sivil NPC'ler. optimizayon için oyuncudan belirli bir metre uzaklıkta hareket etmezler
    * **Güvenlik Görevlileri (`SecurityAI`):** FSM kullanan kullanıcıya alarm durumunda ateş edebilen npcler
* **Dinamik FSM Durumları:** Güvenlik görevlileri `Idle` (Boşta Durma), `Patrol` (Devriye), `Chase` (Kovalama) ve `Attack` (Saldırı) durumları arasında dinamik olarak geçiş yapar.
* **Global Alarm Sistemi:** Oyuncu ana tabloyu çaldığı an PaintingTrigger.cs sayesinde boolean bir değişken aracılığıyla global alarm tetiklenir ve *tüm* güvenlik görevlileri `Patrol` durumunu terk edip oyuncuyu aramaya başlar.
* **Evrensel Can Sistemi Health.cs scripti sayesinde hem oyuncu hem güvenlik hemde ziyaretçiler ortak bir canı paylaşır
* **Komple Oyun Döngüsü:**
    * **Kazanma Koşulu:** Tabloyu çaldıktan sonra (`isAlarmTriggered == true`) olunca kaçış noktasına ulaşmak. (`EscapeZone.cs`) scripti burada devreye giriyor
    * **Kaybetme Koşulu:** Oyuncunun canının 0'a düşmesi (`PlayerUI.cs`) scripti Health.cs can 0 olduğu anda Die() fonksiyonunu çağırarak ekrana game over yazdırır
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
    4.  Işın bir `Collider`'a (Bu durumda bir duvara yada ai a çarpabilir) çarparsa, `GetComponentInParent<Health>()` ile o objenin `Health.cs` script'ini arar.
    5.  Bulursa, `targetHealth.TakeDamage()` fonksiyonunu çağırır.
* **AI - Güvenlik FSM (Finite State Machine):**
    `SecurityAI.cs` script'i `Update()` içinde sürekli olarak `HandleStateTransitions()` (Durum Geçişlerini Yönet) fonksiyonunu çalıştırır. Bu fonksiyon, yapay zekanın "beynidir" ve hangi durumda (`currentState`) olacağına karar verir.

* VisitorAI, SecurityAI a göre daha basit mantıkla çalışır `Idle` (Bekleme) ve `Walking` (Yürüme) arasında rastgele bir zamanlayıcı ile geçiş yapar.
* **Oyun Döngüsü (Kazanma/Kaybetme):**
    1.  **Kaybetme:** `Health.cs` (Player) -> `Die()` -> `OnDie.Invoke()` (Event) -> `PlayerUI.ShowGameOverScreen()` (Metodu dinler).
    2.  **Kazanma:** `PaintingTrigger.cs` (Oyuncu girer) -> `SecurityAI.isAlarmTriggered = true` (Static genel değişken ayarlanır) -> Oyuncu `EscapeZone.cs`'e (Empty bir gameobject isTrigger = True olan bir collidera sahiptir) girer -> `EscapeZone.cs`, `SecurityAI.isAlarmTriggered == true` olduğunu görür -> `PlayerUI.ShowWinScreen()`.

### 2. Tasarlanan Arayüz Sistemleri ("Sayfalar")

Proje, `UnityEngine.UI` (Canvas) sistemi üzerine kurulu 3 ana "sayfa" veya "ekran" durumuna sahiptir:

1.  **Oyun İçi Arayüz (In-Game HUD):**
    * **Can Göstergesi:** `PlayerUI.cs` script'i tarafından yönetilen, oyuncunun `Health.CurrentHealth` değerini anlık olarak gösteren bir `TextMeshPro` yazısı.
    * **Nişangah (Crosshair):** `PlayerUI.cs` tarafından yönetilen bir `Image`. Sadece `isArmed == true` (silahlı) durumdayken görünür.
    * Raycast atılırken oyuncunun silah namlusunun ucundan değilde bu aim in tam ortasından atılır ama Security için Silahın namlusunun ucundan atılır
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
Biz bu proje fikrini ortaya attıktan sonra gerçekte parisde louvre müzesini soymaya çalıştılar sorumluluk kabuk etmiyoruz :)

* Yukarıdaki bahsettiğim oyunlar çok daha karışık Behavihor AI tree mantığını kullanır. Bizim projemiz prototip, okul projesi olduğu için dökümanda belirtildiği gibi FSM yeterli görülmüştür
* **Bizim Çalışmamızın Yeri:** Bu proje, klasik FSM mimarisinin modern bir prototipidir. Global durumu yönetmek için `static bool isAlarmTriggered` kullanması, küçük ölçekli projeler için etkili, ancak büyük ölçekli oyunlar için Daha kapsamlı yaklaşımlar tercih edilir bizimkisi daha basit, tek değişkenli bir yaklaşımdır.

### 4. Kullanılan Yazılımsal Mimariler, Yöntemler ve Teknikler

Proje boyunca birçok temel oyun geliştirme tekniği ve mimarisi uygulanmıştır:

1.  **Sonlu Durum Makinesi (Finite State Machine - FSM):** Hem `SecurityAI` hem de `VisitorAI` script'lerinin çekirdek mantığıdır. `enum AIState` (durumları tanımlamak için) ve `Update()` içindeki bir `switch` (veya `if-else`) bloğu (o anki duruma göre eylemi seçmek için) kullanılarak kodlanmıştır.
2.  **Olay Güdümlü Programlama (Event-Driven Programming):** `Health.cs` script'i ile `PlayerUI.cs` script'i arasındaki iletişim bu şekilde sağlanmıştır. `Health` script'i öldüğünde `OnDie` adında bir Event tetikler. `PlayerUI` script'i bu olayı dinler ve tetiklendiğinde `ShowGameOverScreen()` fonksiyonunu çalıştırır.
3.  **Animasyon Yeniden Hedefleme (Animation Retargeting):** Mixamo'dan indirilen animasyonlar, `Humanoid` (İnsansı) `Rig` (İskelet) tipine ayarlanarak, hem Oyuncu (Player) hem de Güvenlik (Security) modelleri gibi farklı iskelet yapılarında sorunsuzca kullanılmıştır unity bu animasyonları farklı prefablara uydurmayı başarmıştır
4.  **Karışım Ağaçları (Blend Trees):** Önceki PlayerAnimationController de hiçbir şekilde karışım ağacı kullanmamıştık bu sebeple animasyonlar arası geçişler saçma bir hal almıştı hatta bazen karakter durduğu yerde yürüyodu biraz yaptığımız araştırmalarda böyle bir yöntem olduğunu öğrendik sonrada buna göre Controller i güncelledik


### 5. Karşılaşılan Zorluklar ve Çözümler

Bu projeyi geliştirirken bu kadar sorun çıkacağını hiç tahmin etmemiştik
Proje geliştirme süreci, özellikle `Animator` ve `NavMeshAgent` sistemlerinin entegrasyonunda zorlu geçmiştir.

### Genel Zorluk ve Hata listesi
* Oyuncu koşarken karakterin sol ayağı garip bir şekilde sola doğru yamuluyodu bu sorunu çözmek için 2 gün uğraştım ama hiçbir şekilde çözemedim bende sorunu çözmek yerine kamera açısını tıpkı PUBG gibi karakterin ayakları görünmeyecek şekilde ayarladım böylelikle bu sorunun üstü kapatılmış oldu.
* Güvenlikler oyuncu a konumundan b konumuna gidince yeteri kadar hızlı tepki veremiyolardı örneğin oyuncu a konumunda güvenlik b konumunda olsun oyuncu b konumuna gitmeye karar verdiğinde de güvenlik o sırada a konumuna gitmeye karar versin yarıyola karşılaşsalar bile güvenlik gitmekten vazgeçip oyuncuya ateş etme durumuna geçmiyodu onu birtürlü çözemedim bende sorunu güvenliğin hızını arttırmakta buldum, güvenlikler inanılmaz hızlı koşuyolar ilk commitlere göre
* Işıklandırmaları ayarlaması çok zor oldu haritanın ana ışık kaynağını vesaire tamamen kapattım ve harita komple karanlık oldu, tek tek karanlık yerleri aydınlattım böylelikle daha doğal bir görünüm oldu
* Sol ayak sorunu harici Güvenlik ve Ziyaretçilerde ölme animasyonu garip bir şekilde oynuyodu uzun süre uğraşmama rağmen onu çözemedim
* Bazen ziyaretçiler girmemeleri gereken yerlere bir şekilde girmeyi başarıyolar neden bilmiyorum collider ile tamamen kapattım oraları ama yinede girmeyi başarıyolar onu çözemedim
* Yine bazı ziyaretçiler garip bir şekilde collider özelliğini kaybedip sağa-sola doğru çok hızlı hareket edebiliyolar normalde karakter animasyonunu ayarlarken Apply root motion özelliği işaretli ise böyle bir davranış sergilerdi ama yaklaşık 5 kere falan kontrol ettim bu özellik kapalı olmasına rağmen hala ziyaretçilerden bazıları garip garip hareketler yapıyor
* Oyun başında tüm karakterlerden bir ölme sesi , tablo çalınıncada tüm karekterlerden bir silah ateş sesi geliyo onuda çözemedim

