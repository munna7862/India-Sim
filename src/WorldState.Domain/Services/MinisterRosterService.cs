using WorldState.Domain.Models;

namespace WorldState.Domain.Services;

public record MinisterCandidate(
    string Id,
    string Name,
    MinistryType Ministry,
    double Competence,
    decimal HiringFee,
    decimal MonthlySalary,
    string Bio
);

public static class MinisterRosterService
{
    private static readonly Dictionary<MinistryType, List<MinisterCandidate>> CandidatesByMinistry = new()
    {
        [MinistryType.Health] = new()
        {
            new("CAND_HLTH_01", "Dr. Soumya Swaminathan", MinistryType.Health, 0.92, 20_000_000m, 350_000m, "Renowned clinical epidemiologist with vast global health administrative experience."),
            new("CAND_HLTH_02", "Dr. Rajesh Kotecha", MinistryType.Health, 0.78, 12_000_000m, 220_000m, "Public healthcare systems reformer and traditional medicine integration specialist."),
            new("CAND_HLTH_03", "Dr. Randeep Guleria", MinistryType.Health, 0.88, 18_000_000m, 300_000m, "Distinguished pulmonologist and former hospital director.")
        },
        [MinistryType.Education] = new()
        {
            new("CAND_EDU_01", "Prof. K. Kasturirangan", MinistryType.Education, 0.94, 25_000_000m, 400_000m, "Architect of national education curriculum reforms and academic research vision."),
            new("CAND_EDU_02", "Prof. Anil Sahasrabudhe", MinistryType.Education, 0.82, 15_000_000m, 250_000m, "Technical education reformer and engineering accreditation leader."),
            new("CAND_EDU_03", "Dr. Manjul Bhargava", MinistryType.Education, 0.86, 18_000_000m, 280_000m, "Mathematician promoting foundational numeracy and science education.")
        },
        [MinistryType.Infrastructure] = new()
        {
            new("CAND_INFRA_01", "E. Sreedharan", MinistryType.Infrastructure, 0.96, 30_000_000m, 500_000m, "Celebrated master engineer known for punctuality, rapid transit, and mega-projects."),
            new("CAND_INFRA_02", "Vinayak Chatterjee", MinistryType.Infrastructure, 0.80, 14_000_000m, 240_000m, "Infrastructure finance advisor and public-private partnership strategist."),
            new("CAND_INFRA_03", "Sudhir Rao", MinistryType.Infrastructure, 0.75, 10_000_000m, 180_000m, "Highway network logistics and freight corridor project manager.")
        },
        [MinistryType.Defense] = new()
        {
            new("CAND_DEF_01", "Gen. Manoj Pande", MinistryType.Defense, 0.92, 25_000_000m, 450_000m, "Strategic combined arms commander focused on rapid force modernization."),
            new("CAND_DEF_02", "Adm. R. Hari Kumar", MinistryType.Defense, 0.85, 18_000_000m, 320_000m, "Maritime security expert emphasizing blue-water presence and indigenization."),
            new("CAND_DEF_03", "Air Chief Marshal V.R. Chaudhari", MinistryType.Defense, 0.88, 20_000_000m, 360_000m, "Aerospace doctrine specialist advocating indigenous fighter procurement.")
        },
        [MinistryType.Finance] = new()
        {
            new("CAND_FIN_01", "Dr. Raghuram Rajan", MinistryType.Finance, 0.95, 30_000_000m, 500_000m, "Acclaimed macroeconomic scholar with deep central banking and monetary authority."),
            new("CAND_FIN_02", "Dr. Arvind Subramanian", MinistryType.Finance, 0.86, 20_000_000m, 340_000m, "Fiscal policy and international trade strategist."),
            new("CAND_FIN_03", "Uday Kotak", MinistryType.Finance, 0.84, 18_000_000m, 300_000m, "Commercial banking veteran with sharp debt management and capital markets acumen.")
        },
        [MinistryType.InternalAffairs] = new()
        {
            new("CAND_INT_01", "Ajit Doval", MinistryType.InternalAffairs, 0.95, 30_000_000m, 500_000m, "Premier security strategist and intelligence architect."),
            new("CAND_INT_02", "Praveen Sood", MinistryType.InternalAffairs, 0.82, 15_000_000m, 260_000m, "Law enforcement administrator focusing on police modernization and cyber forensics."),
            new("CAND_INT_03", "Subodh Kumar Jaiswal", MinistryType.InternalAffairs, 0.79, 12_000_000m, 200_000m, "Senior investigative director specializing in federal anti-corruption and stability.")
        },
        [MinistryType.SecurityIntelligence] = new()
        {
            new("CAND_SEC_01", "Ravi Sinha", MinistryType.SecurityIntelligence, 0.90, 25_000_000m, 400_000m, "Foreign intelligence operative and cross-border threat analyst."),
            new("CAND_SEC_02", "Tapan Deka", MinistryType.SecurityIntelligence, 0.87, 20_000_000m, 320_000m, "Counter-terrorism operations director with proven domestic infiltration track record."),
            new("CAND_SEC_03", "S.M. Sahai", MinistryType.SecurityIntelligence, 0.80, 14_000_000m, 230_000m, "Strategic security analyst focused on border surveillance and cyber intelligence.")
        },
        [MinistryType.IndustryConstruction] = new()
        {
            new("CAND_IND_01", "N. Chandrasekaran", MinistryType.IndustryConstruction, 0.93, 28_000_000m, 450_000m, "Industrial conglomerate leader specializing in manufacturing supply chain scalability."),
            new("CAND_IND_02", "Baba Kalyani", MinistryType.IndustryConstruction, 0.85, 18_000_000m, 310_000m, "Heavy forge industrialist and defense manufacturing champion."),
            new("CAND_IND_03", "Anand Mahindra", MinistryType.IndustryConstruction, 0.88, 22_000_000m, 380_000m, "Automotive and aerospace industrialist fostering domestic manufacturing.")
        },
        [MinistryType.Environment] = new()
        {
            new("CAND_ENV_01", "Sunita Narain", MinistryType.Environment, 0.91, 20_000_000m, 320_000m, "Leading environmental activist, water conservationist and green policy advocate."),
            new("CAND_ENV_02", "Dr. Vandana Shiva", MinistryType.Environment, 0.80, 14_000_000m, 220_000m, "Biodiversity protection and sustainable agriculture campaigner."),
            new("CAND_ENV_03", "Madhav Gadgil", MinistryType.Environment, 0.86, 17_000_000m, 270_000m, "Ecologist renowned for western ghats conservation and ecological zoning.")
        },
        [MinistryType.Justice] = new()
        {
            new("CAND_JUST_01", "Justice B.N. Srikrishna", MinistryType.Justice, 0.92, 24_000_000m, 380_000m, "Distinguished jurist known for data privacy law and judicial modernization."),
            new("CAND_JUST_02", "Fali S. Nariman", MinistryType.Justice, 0.94, 28_000_000m, 450_000m, "Eminent constitutional jurist with unparalleled mastery of legal ethics."),
            new("CAND_JUST_03", "Gopal Subramanium", MinistryType.Justice, 0.83, 16_000_000m, 260_000m, "Senior counsel committed to judicial backlog clearance and legal reform.")
        },
        [MinistryType.Culture] = new()
        {
            new("CAND_CUL_01", "Kapila Vatsyayan", MinistryType.Culture, 0.89, 16_000_000m, 250_000m, "Art historian and premier guardian of national cultural heritage."),
            new("CAND_CUL_02", "Shashi Tharoor", MinistryType.Culture, 0.87, 18_000_000m, 300_000m, "Literary author and diplomatic cultural ambassador."),
            new("CAND_CUL_03", "Dr. Karan Singh", MinistryType.Culture, 0.84, 14_000_000m, 220_000m, "Statesman promoting interfaith dialogue and philosophical traditions.")
        },
        [MinistryType.Tourism] = new()
        {
            new("CAND_TOUR_01", "Deep Kalra", MinistryType.Tourism, 0.88, 18_000_000m, 300_000m, "Digital travel pioneer transforming international tourist acquisition."),
            new("CAND_TOUR_02", "Priya Paul", MinistryType.Tourism, 0.82, 14_000_000m, 220_000m, "Hospitality innovator promoting boutique luxury and eco-tourism corridors."),
            new("CAND_TOUR_03", "Amitabh Kant", MinistryType.Tourism, 0.91, 24_000_000m, 390_000m, "Branding mastermind behind iconic global tourism campaigns.")
        },
        [MinistryType.Sports] = new()
        {
            new("CAND_SPT_01", "Abhinav Bindra", MinistryType.Sports, 0.93, 22_000_000m, 350_000m, "Olympic gold medalist and athletic high-performance training architect."),
            new("CAND_SPT_02", "Pullela Gopichand", MinistryType.Sports, 0.90, 20_000_000m, 320_000m, "Legendary coach championing grassroots sports academies and physical culture."),
            new("CAND_SPT_03", "Mary Kom", MinistryType.Sports, 0.85, 16_000_000m, 250_000m, "World champion athlete inspiring youth fitness and boxing infrastructure.")
        }
    };

    public static IReadOnlyList<MinisterCandidate> GetCandidates(MinistryType ministry)
    {
        return CandidatesByMinistry.TryGetValue(ministry, out var list)
            ? list
            : Array.Empty<MinisterCandidate>();
    }
}
