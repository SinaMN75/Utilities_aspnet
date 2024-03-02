namespace Utilities_aspnet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScrapperController : ControllerBase {
	[HttpGet("GetInstaPostRapidApi/{username}")]
	public static string GetInstaPostRapidApi(string username) {
		RestClient client = new($"https://rapidapi.sinamn75.com/api/Scrapper/GetInstaPostRapidApi/{username}") { RemoteCertificateValidationCallback = (_, _, _, _) => true };
		RestRequest request = new(Method.GET);
		IRestResponse response = client.Execute(request);
		return response.Content;
	}

	[HttpGet("GetInstaPostRapidApiTest")]
	public static string GetInstaPostRapidApiTest() =>
		"{\"instagramPosts\":[{\"desctiption\":\"\",\"images\":[\"https://rapidapi.sinamn75.com/ff24fbae-a085-4535-a1a8-16cc92796352.png\"]},{\"desctiption\":\"تولد امساااااال شاید بهترین تولد عمرم بود. بهترین آدمای زندگیم کنارم بودن و بهترین کادوهامو گرفتم. مرسی از همتوننن\ud83d\ude0d\",\"images\":[\"https://rapidapi.sinamn75.com/cfdce873-2a4b-4830-a13b-7c693b55866f.png\",\"https://rapidapi.sinamn75.com/e6cf2981-239b-402a-aa80-a1d27cc6fc60.png\",\"https://rapidapi.sinamn75.com/0d7e9640-c62f-4512-b822-01c14dbbe7f8.png\",\"https://rapidapi.sinamn75.com/a01c5a7c-160d-4feb-9c48-c29966dd3516.png\",\"https://rapidapi.sinamn75.com/ac7b1c07-b38e-46a2-9139-255dbc26f9ce.png\"]},{\"desctiption\":\"God!!!\\n#needforspeed\",\"images\":[\"https://rapidapi.sinamn75.com/ba6fcee8-c0f2-4dab-8fd4-c4b87023506e.png\"]},{\"desctiption\":\"\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\",\"images\":[\"https://rapidapi.sinamn75.com/9b2c0b06-1985-4ce1-9c60-0f502d507f8a.png\",\"https://rapidapi.sinamn75.com/14b5fc43-e47e-4d16-9462-559e84693b11.png\",\"https://rapidapi.sinamn75.com/ea8bb378-2ee7-4bfe-8663-21e2f6014ef9.png\"]},{\"desctiption\":\"اینم سورپرایز روز مهندس\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\",\"images\":[\"https://rapidapi.sinamn75.com/749169c7-e945-4210-8f3a-7b7559ef6743.png\",\"https://rapidapi.sinamn75.com/ac1c6b27-0202-4980-8e9a-4706e1242aca.png\",\"https://rapidapi.sinamn75.com/946c4827-031d-4ebb-8d02-893cbd0ccaf3.png\"]},{\"desctiption\":\"\u2764\ufe0f\u2764\ufe0f\u2764\ufe0f\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83e\udd70\ud83e\udd70\ud83e\udd70 #valentine\",\"images\":[\"https://rapidapi.sinamn75.com/11c76918-9478-4f6a-a8d3-bd4addeb11c5.png\",\"https://rapidapi.sinamn75.com/137129c8-6414-412c-a10b-1e4a0b3eb318.png\",\"https://rapidapi.sinamn75.com/2ffb4d57-eae5-40c4-833e-acff783ef90e.png\"]},{\"desctiption\":\"2022 Recap...\",\"images\":[]},{\"desctiption\":\"\",\"images\":[\"https://rapidapi.sinamn75.com/6a6c24fd-d2ec-4a90-b809-3eccd962293b.png\",\"https://rapidapi.sinamn75.com/32e4e08a-1fcb-4476-941e-a031cbacbea0.png\",\"https://rapidapi.sinamn75.com/ed15a9ca-5fff-4d94-b319-4a626785d776.png\",\"https://rapidapi.sinamn75.com/8e76c5c7-c0cb-4e75-aa1f-f93b3dce21b6.png\",\"https://rapidapi.sinamn75.com/e4121944-c09f-4470-9604-7c0fa86111a0.png\",\"https://rapidapi.sinamn75.com/62199c7d-6c08-45b3-ac41-ae7e2da1b895.png\",\"https://rapidapi.sinamn75.com/9517abce-f408-4979-bfbd-9c317386bc0f.png\",\"https://rapidapi.sinamn75.com/3390c682-2421-4e75-8bd5-aa8ac6283d17.png\",\"https://rapidapi.sinamn75.com/3f31e5e9-8b9f-4d22-bf16-b1703a4d64de.png\"]},{\"desctiption\":\"\",\"images\":[]},{\"desctiption\":\"\",\"images\":[\"https://rapidapi.sinamn75.com/22a42bf2-7fa8-4a0e-9f6d-900d74d320ba.png\"]},{\"desctiption\":\"میکاپهههه\ud83d\ude0d\",\"images\":[]},{\"desctiption\":\"تولد لیانااااااا\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\",\"images\":[\"https://rapidapi.sinamn75.com/23b6341b-8e07-4397-9eea-995a41fc147f.png\",\"https://rapidapi.sinamn75.com/c433df1e-2140-4b8d-9bac-33839d701d4a.png\",\"https://rapidapi.sinamn75.com/d56ee094-27f5-4b02-bac2-6c941da189be.png\"]},{\"desctiption\":\"یه روز فوق\u200cالعاده. یه تجربه عالی\ud83d\udc4d\ud83d\udc4c\ud83d\ude42\",\"images\":[\"https://rapidapi.sinamn75.com/7327d77e-69fb-4ec1-b69a-0d019fa3cacf.png\",\"https://rapidapi.sinamn75.com/40cfae51-1c6f-4eb8-a041-16be6c6aeb92.png\",\"https://rapidapi.sinamn75.com/f420c514-dc28-482e-99df-b0fcab6be0e1.png\",\"https://rapidapi.sinamn75.com/3e7e608f-a7e3-4d54-8078-22784de01506.png\",\"https://rapidapi.sinamn75.com/344aa118-bd64-44b4-a0b0-e8ee57918aad.png\",\"https://rapidapi.sinamn75.com/4777002e-b6eb-4cfa-8836-46c97b007793.png\"]},{\"desctiption\":\"یاد دانشگاه بخیر. در این حد استادامونو دوست داشتیم\ud83d\ude02\ud83d\ude02\ud83d\ude02. سال ۹۳\",\"images\":[\"https://rapidapi.sinamn75.com/b71c155d-d442-445f-a678-b9bb1532a243.png\"]},{\"desctiption\":\"وقتی جواب ازمایشتو میخوای تو گوگل ترنسلیت ترجمه کنی\ud83d\ude10\",\"images\":[\"https://rapidapi.sinamn75.com/24cba43f-6977-4095-8103-aeb035f004d0.png\"]},{\"desctiption\":\"Whyyyyyy?????\\n\\n#ea#electronicarts #needforspeed #xbox\",\"images\":[]},{\"desctiption\":\"از اولین انشا\\nفقط یک جمله یادم مانده\\nبابا اگر نان داد\\nتاوان هم فراوان داد...\\nروز پدر مبارک\",\"images\":[\"https://rapidapi.sinamn75.com/6b9622d7-4ab7-48b8-8d05-ea4ea8522afe.png\"]},{\"desctiption\":\"\",\"images\":[\"https://rapidapi.sinamn75.com/ac8d532d-825b-455b-acf8-191341ea70fe.png\"]},{\"desctiption\":\"از جاهای کوچیک شروع میشه. میرسه اون بالا بالاها\ud83d\ude80\ud83d\ude80\ud83d\ude80\",\"images\":[\"https://rapidapi.sinamn75.com/a00f0359-0e15-4a98-b3dc-48fede6e071f.png\",\"https://rapidapi.sinamn75.com/cd8af12b-4e43-41ec-852d-02f7c21134e3.png\",\"https://rapidapi.sinamn75.com/1447c11c-2405-46ed-b922-c23ec40e86f8.png\",\"https://rapidapi.sinamn75.com/583d7316-ef22-4337-8387-6db028b78d61.png\",\"https://rapidapi.sinamn75.com/68597a1d-e1c4-49b3-a1ba-59688d3bad05.png\"]},{\"desctiption\":\"ناهار بعد پیروزی\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\",\"images\":[\"https://rapidapi.sinamn75.com/8af036de-5159-4378-94da-88995aa9ee0b.png\",\"https://rapidapi.sinamn75.com/ec2a77d1-2d36-45b4-872e-033a68ef7be1.png\",\"https://rapidapi.sinamn75.com/8389cf08-6c5f-4df5-9b82-a5c4b778cf05.png\"]},{\"desctiption\":\"Hey CJ\ud83e\udd2a\ud83e\udd2a\ud83e\udd2a\",\"images\":[\"https://rapidapi.sinamn75.com/099c4b43-c390-42d5-bd23-18533444cd25.png\"]},{\"desctiption\":\"یه روز خیلی کاری\ud83d\ude02\ud83d\ude02\ud83d\ude02\",\"images\":[\"https://rapidapi.sinamn75.com/63be0ca0-be6d-4d76-91fe-0d0d174609b6.png\"]},{\"desctiption\":\"\ud83c\udf2d\ud83c\udf2f\ud83c\udf2e\ud83c\udf54\ud83e\udd57\",\"images\":[\"https://rapidapi.sinamn75.com/c593632d-3469-4d74-a5cf-68b4ffb5b059.png\",\"https://rapidapi.sinamn75.com/b684555f-6f8a-46a0-bdd5-7dcd48e1be2e.png\",\"https://rapidapi.sinamn75.com/5376f581-d853-479a-9e93-ad7dfd989c83.png\"]},{\"desctiption\":\"ناهار بعد\u200cشکست کرونا\ud83d\ude02\ud83d\ude02\ud83d\ude02\",\"images\":[\"https://rapidapi.sinamn75.com/880cd5d7-c8dd-434f-9c68-b7477e38a9e5.png\",\"https://rapidapi.sinamn75.com/874e664a-ef40-4d4e-a2b0-774e9eeb2cfe.png\",\"https://rapidapi.sinamn75.com/d701ff88-0012-42e5-91a4-141586f97c6b.png\",\"https://rapidapi.sinamn75.com/2b9553e6-0a80-4080-9630-ffe00ff7042f.png\"]},{\"desctiption\":\"\ud83c\udf77\ud83c\udf78\ud83c\udf79\",\"images\":[\"https://rapidapi.sinamn75.com/6513b3b8-1653-48d4-a590-7b5acd816b9f.png\"]},{\"desctiption\":\"\ud83c\udf54\ud83c\udf2d\ud83c\udf2e\ud83c\udf2f\ud83e\udd59\ud83e\udd6a\ud83e\udd57\",\"images\":[\"https://rapidapi.sinamn75.com/be04fdde-5bd6-437e-93e8-fbcd6334c578.png\"]},{\"desctiption\":\"بازم \ud83c\udf68\ud83c\udf67\ud83c\udf70\ud83c\udf82\",\"images\":[\"https://rapidapi.sinamn75.com/3927b196-11b5-4f56-97e8-2517a724d7ed.png\"]},{\"desctiption\":\"\ud83c\udf70\ud83c\udf67\ud83c\udf68\ud83c\udf82\ud83c\udf7f\",\"images\":[\"https://rapidapi.sinamn75.com/6b86d536-38be-4bd2-bb22-2ed877f9b267.png\"]},{\"desctiption\":\"یه روز خوب بعد\u200c از چند روز بد…\",\"images\":[\"https://rapidapi.sinamn75.com/24c479e8-3f96-43ac-a7ae-167b6ace32a2.png\"]},{\"desctiption\":\"برو بچه های اپتک\",\"images\":[\"https://rapidapi.sinamn75.com/00804cfd-f702-4b7e-b819-9566ebb2d80d.png\"]},{\"desctiption\":\"لیانااااا\u2764\ufe0f\u2764\ufe0f\u2764\ufe0f\u2764\ufe0f\u2764\ufe0f\",\"images\":[\"https://rapidapi.sinamn75.com/ea45abfa-b924-4e15-b105-cc264d672c24.png\"]},{\"desctiption\":\"\u2764\ufe0f\u2764\ufe0f\u2764\ufe0f\u2764\ufe0f\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\",\"images\":[\"https://rapidapi.sinamn75.com/c82c4f49-a7e6-4ec0-992d-c30f070901b9.png\"]},{\"desctiption\":\"تولد تولد\ud83c\udf39\ud83c\udf39\ud83c\udf39\u2764\ufe0f\u2764\ufe0f\u2764\ufe0f\u2764\ufe0f\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\",\"images\":[\"https://rapidapi.sinamn75.com/166f2192-dfc1-4ee8-a78e-003f6ed2da3e.png\"]},{\"desctiption\":\"یه روز خوب دیگه\ud83e\uddcb\",\"images\":[\"https://rapidapi.sinamn75.com/633ae8c2-832a-4fe8-8743-d08c51caac0d.png\",\"https://rapidapi.sinamn75.com/92d4ccf5-0694-4723-bf1f-aa582c9e9e3d.png\",\"https://rapidapi.sinamn75.com/45474196-6671-4096-a19e-2247a32548ea.png\",\"https://rapidapi.sinamn75.com/7ea4f34b-c9b7-47f8-b0bd-b1cab530bf4b.png\"]},{\"desctiption\":\"\ud83e\udd64\ud83e\uddcb\ud83e\udd5b\u2615\ufe0f\",\"images\":[\"https://rapidapi.sinamn75.com/d994ff60-7d9d-4811-a25b-427bce703db7.png\"]},{\"desctiption\":\"یه روز خوب با یچه های خوب @apptech_lifestyle\",\"images\":[\"https://rapidapi.sinamn75.com/e0cb2d8c-db5d-4c09-9716-b56a4bd6aed6.png\",\"https://rapidapi.sinamn75.com/8fb335d2-411f-417d-9573-f9591c6a4b0b.png\",\"https://rapidapi.sinamn75.com/9c5eae50-1080-4331-b8ca-f9b86a4e37f6.png\",\"https://rapidapi.sinamn75.com/f674660c-01cf-49b3-921f-8094cf6bc5c3.png\"]},{\"desctiption\":\"یعنی دست به طلا بزنم خاکستر میشه\ud83d\ude10\ud83d\ude10\ud83d\ude10\ud83d\ude10\ud83d\ude10\ud83d\ude10\ud83d\ude10\ud83d\ude10#bitcoins #dogecoin #cryptocurrency #crypto\",\"images\":[\"https://rapidapi.sinamn75.com/114b03f1-b631-4062-92d5-8a05b5bd7a3e.png\"]},{\"desctiption\":\"ایشالا امسال با خوبیاش پارسال و بشوره ببره #نوروز #عید #سالنومبارک\",\"images\":[]},{\"desctiption\":\"Video by @cee_roo\",\"images\":[]},{\"desctiption\":\"عیدی زیبا و جذاب apptech\u2764\ufe0f\u2764\ufe0f\u2764\ufe0f \\n#dev #apptech\",\"images\":[\"https://rapidapi.sinamn75.com/d5283d64-b48b-491f-9d7e-474117eb6162.png\",\"https://rapidapi.sinamn75.com/f26f439a-508d-4052-ad0e-a97a2fddd540.png\",\"https://rapidapi.sinamn75.com/86cc2e33-1929-4b59-b37e-5527aeb104d0.png\",\"https://rapidapi.sinamn75.com/0a622ca5-06d5-4746-885f-b21c86034777.png\"]},{\"desctiption\":\"یه روز خوب\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\",\"images\":[\"https://rapidapi.sinamn75.com/b1521d88-eb0d-4112-8e4d-edfd97610c0b.png\",\"https://rapidapi.sinamn75.com/148c2cd5-3a74-4b4a-8b27-d586afce53b4.png\",\"https://rapidapi.sinamn75.com/1c66778d-09a4-43fb-8ac4-be59c9f281c3.png\"]},{\"desctiption\":\"دوستان و همکاران مجموعه apptech \ud83c\udf89\ud83c\udf89\ud83c\udf89\",\"images\":[\"https://rapidapi.sinamn75.com/f3438e2d-84be-4151-84ab-1f5d4ca0d93e.png\"]},{\"desctiption\":\"دور همی بعد سرکار خستگی نمیزاره واسه ادم\",\"images\":[\"https://rapidapi.sinamn75.com/57d296a0-e8fe-487a-80d9-93a239e71089.png\",\"https://rapidapi.sinamn75.com/3bb62241-55a1-490e-980b-ca09c4619d9b.png\",\"https://rapidapi.sinamn75.com/f4e27903-c60a-4e6f-bbff-f84bd701dbb6.png\"]},{\"desctiption\":\"بعد یه مدت بکوب کار کردن خوشگذرونی خیلی میچسپه\",\"images\":[\"https://rapidapi.sinamn75.com/eb35dfb0-5489-4996-956f-d687b29cf5e4.png\",\"https://rapidapi.sinamn75.com/eb1a990a-2c81-4098-bcc5-84d36f9c0ab8.png\",\"https://rapidapi.sinamn75.com/9a1f07c1-d256-4528-af7c-222f264ba3e8.png\",\"https://rapidapi.sinamn75.com/4ae78bdf-4ca7-458c-8f92-32610ff24672.png\",\"https://rapidapi.sinamn75.com/dc4638e0-479c-4512-88a0-b58b847ac80e.png\"]},{\"desctiption\":\"\u2764\ufe0f\u2764\ufe0f\u2764\ufe0f\",\"images\":[\"https://rapidapi.sinamn75.com/32d40225-7463-45c9-a99c-06d297c1534f.png\",\"https://rapidapi.sinamn75.com/0858ab11-3237-45bc-b89e-5e11b8b73671.png\",\"https://rapidapi.sinamn75.com/4c6115ff-ca48-4ebb-b109-058d9b36d423.png\"]},{\"desctiption\":\"\",\"images\":[\"https://rapidapi.sinamn75.com/25a4d7c5-a9fe-447c-a975-a0c1a284569c.png\"]},{\"desctiption\":\"تولدت مبارک. دو سالگی لیاناااا \ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\ud83d\ude0d\",\"images\":[]},{\"desctiption\":\"\",\"images\":[\"https://rapidapi.sinamn75.com/75502cc0-a71b-41e5-84d8-0751a7ae3232.png\"]}]}";
}