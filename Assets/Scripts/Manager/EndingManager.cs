using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class EndingManager
{
    private Role player;
    private Role people;
    private Role world;
    
    public EndingManager()
    {
        player = GameManager.Instance.RoleManager.GetRole(RoleType.Player);
        people = GameManager.Instance.RoleManager.GetRole(RoleType.People);
        world = GameManager.Instance.RoleManager.GetRole(RoleType.World);
    }

    public string GenerateEndingSummary()
    {
        float faith = player.GetStat("信奉度");
        float mystery = player.GetStat("神秘性");
        float divinity = player.GetStat("神格性");
        float development = people.GetStat("发展度");
        float conformity = people.GetStat("顺从正统度");
        float believerRatio = people.GetStat("信众数")/people.GetStat("人口数");
        float chaos = world.GetStat("动荡度");
        float calamity = world.GetStat("灾丰积累度");

        var sections = new List<string>
        {
            GetFaithEnding(faith),
            GetMysteryEnding(mystery),
            GetDivinityEnding(divinity),
            GetSocietyEnding(development, mystery),
            GetFollowerEnding(conformity, believerRatio),
            GetWorldEnding(chaos, calamity)
        };

        StringBuilder sb = new StringBuilder();

// 卡牌名称 - 大字号加粗
        sb.AppendLine($"<b><size=115%>这就是你的结局了</size></b>");

// 条目列表
        foreach (var section in sections)
        {
            sb.AppendLine($"<b><size=80%>{sections}</size></b>");
        }

        return sb.ToString();
    }

    private string GetFaithEnding(float value)
    {
        if (value <= -50) return "你从未祈祷，也未曾信过。你只是在使用信仰——这不是你的归属，而是你打造的工具，你披着神明的皮囊，把信仰当作是钩子、武器、毒药。你不是牧者，是屠夫。你引导信众走入迷雾，却从未陪他们踏出第一步。";
        if (value <= -10) return "你始终清醒。信仰对你而言是可以被操控的结构，不必膜拜，也无需否认。你给它以框架，赋它以目的，然后放手让其运行。你站在外侧，冷眼看着那些相信你的人沉迷、颤抖、堕落或飞升。";
        if (value <= 10) return "你你维持了某种距离。既不奉之为神，也不将其彻底弃绝。你穿梭在仪式与怀疑之间，是教义的编织者，也是怀疑的种子。你既是建立者，也是见证者。";
        if (value <= 49) return "起初你只是布道者，但你逐渐被自己创下的体系所吸引。你开始相信自己的信徒眼中所见的神迹，开始理解那些愚昧的狂热中，藏着某种你无法否认的真实。信仰不是你的工具，而成了你的镜子。";
        return "你已沉入教义之中。你不再扮演神职者，而是成为真正的信徒。你的每一个决定都源于信仰的召唤，你将真理奉为绝对，将异端斥为虚妄。你成为你自己创造的神明的奴仆，自愿为其燃尽。";
    }

    private string GetMysteryEnding(float value)
    {
        if (value <= -50) return "你的信仰如石碑，如规则本身。没有神迹、没有祈祷回应，只有严密逻辑与行为规范。你创造的，是一套思想机器，而不是宗教。这是一种制度化的信念，一种无神的信仰。";
        if (value <= -10) return "你偶尔使用象征，偶尔使用仪式，但你更依赖组织、秩序与理性说服。你的信仰像是一部未完成的哲学著作，被不断注解，而非膜拜。";
        if (value <= 10) return "神秘与理性在你手中交织。你使用神迹建立权威，又借组织巩固教义。你的教派既能引发恐惧，又能提供解释。既能催生奇迹，也能维持秩序。";
        if (value <= 50) return "你拥抱了神秘。你让异象成为现实，让不可知的风暴席卷众人心智。你把信仰塑造成一种体验，一种梦魇般的真实。";
        return "你抛弃了理性，只留下神秘的恐惧。你制造奇迹、引导异象、重构现实逻辑。你不是教主，是神迹的引路人，是混沌的播种者。你信仰的是无名者，而你自己，也成为了其中一员。";
    }

    private string GetDivinityEnding(float value)
    {
        if (value <= -50) return "没有人知道你是谁。你隐匿于信仰深处，从不暴露自己的真实意图。你的身影被彻底抹除，连名字也被教义所替代。你不是神明，不是先知，只是那根推动齿轮的无形之手。";
        if (value <= -10) return "你是信仰背后的剪影，是理念的缔造者，却从不居功。你给予信徒方向，却不接受崇拜。你更像一位策划者，而非救世主。";
        if (value <= 10) return "有时你现身，有时你消失。你是神与人的过渡，是圣典中的沉默章节，是象征与实体之间的桥梁。";
        if (value <= 50) return "你成了他们崇拜的中心。你的形象被刻在石壁上，你的言语被抄录成圣书。你不再只是引导者，你是答案本身。";
        return "你自称为神。你要求祭祀，接受信仰，享受膜拜。你不仅创造了信仰，也成为信仰的对象。你的死亡被称为升华，你的疯狂被称为启示。你已不再是人。";
    }

    private string GetSocietyEnding(float development, float mystery)
    {
        if (development >= 50 && mystery <= -10) return "你构建了一座理性的神权国度。人们在秩序中归信，信仰成为法律的延伸，仪式成为社会契约。你用宗教重建了文明。";
        if (development >= 50 && mystery >= 10) return "你制造了一种制度化的神秘政体。每一次祈祷都有回应，每一个岗位都有灵力标记。你建立了一个以神迹为结构的统治体系。";
        if (development < 50 && mystery <= -10) return "你播撒的是思想，而非体制。你掀起了一场思想风暴，却没有留下可继承的实体。你的信仰如流星，闪耀但短暂。";
        return "你带来的不是秩序，而是感染。你的信仰在废墟间疯长，无需教条、无需结构，只需狂热与崇拜。你播下的是混乱中的火种，是神秘的瘟疫。";
    }

    private string GetFollowerEnding(float conformity, float ratio)
    {
        if (conformity >= 50 && ratio >= 50) return "你的信仰成为了唯一的秩序。街头巷尾皆有你的雕像，孩子出生时被赋名于你的圣典。你已成神，而你的信徒已成国民。";
        if (conformity >= 50 && ratio < 50) return "你的信仰尚未广泛传播，但它如钢铁般严密。每一位信徒都无比忠诚，每一项仪式都被严格执行。";
        if (conformity < 50 && ratio >= 50) return "你的信仰广泛传播，却未被接纳为正统。它像洪水一样蔓延，却也引发了反抗与清洗。你的信徒狂热，你的敌人更甚。";
        return "你是一位边缘的先知，在混乱中低声传道。信仰的火苗尚未燃起，世界还未准备好理解你的真理。";
    }

    private string GetWorldEnding(float chaos, float calamity)
    {
        if (chaos <= 10 && calamity <= 10) return "世界在你的引导下趋于安定。你给予了人们秩序，也给予了意义。你的信仰被历史铭记，被后世继承。";
        if (chaos <= 50) return "你的教义带来了巨变。有人成为信徒，有人焚毁你的圣典。你既是拯救者，也是动荡之源。你的信仰将继续存在，但不再以你期望的方式演进。";
        if (chaos <= 100 && calamity <= 100) return "你撕裂了世界的逻辑，解构了现实。你的信仰不仅改变了人心，也改变了自然本身。世界在你身后动荡不休，如同你临终时的神迹。";
        return "你唤醒了沉睡之物，让现实崩解。你的死亡不是终结，而是开启——你成了灾厄本身。";
    }
}
