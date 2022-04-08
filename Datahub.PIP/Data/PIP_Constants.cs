﻿using Elemental.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datahub.Portal.Data.PIP
{
    public class PIP_Constants
    {
        public static readonly string[] NA = { "N/A" };


        public static readonly string[] FIVE_OPTIONS_MANDATORY = { "TO BE COMPLETED", "1", "2", "3", "4", "5" };
        public static readonly string[] FIVE_OPTIONS = { "1", "2", "3", "4", "5" };
        public static readonly string[] TIMELINE = { "Ongoing", "Future" };
        public static readonly string[] YESNO = { "Yes", "No" };
        public static readonly string[] NOT_REPORTABLE_INDICATOR = { "Indicator outdated", "Indicator or data quality issues", "Program ending", "Methodology needs updating", "Data not available", "Other" };

        public static readonly string[] ONGOING_ACTIVITIES = { "Risk Analysis", "Risk controls", "Measurement and Communication", "No monitoring activities", "Other" };
        public static readonly string[] ONGOING_ACTIVITIES_TIMEFRAME = { "Daily",
                                                    "Weekly",
                                                    "Bi-weekly",
                                                    "Monthly",
                                                    "Bi-monthly",
                                                    "Quarterly", 
                                                    "Semi-annually", 
                                                    "Annually", 
                                                    "Ad-hoc", 
                                                    "Other", 
                                                    "N/A"
        };
        


        public static readonly string[] EQUITY_SEEKING_GROUPS = { "Indigenous Peoples", "LGBTQ2S+", "People with disabilities", "Black people and racialized people", "Women", 
                                                                    "Underrepresented groups including immigrants", "older adults", " people living in poverty", "rural/remote residents and language minorities" };

        public static readonly string[] CORE_RESPONSIBILITY = { "R1 Canadians have access to cutting-edge research to inform decisions on the management  of natural resources",
                                                                "R2 Communities and officials have the tools to safeguard Canadians from natural hazards and explosives",
                                                                "R3 Communities and industries are adapting to climate change",
                                                                "R4 Natural resource sectors are innovative",
                                                                "R5 Clean technologies and energy efficiencies enhance economic performance",
                                                                "R6 Canada’s natural resources are sustainable",
                                                                "R7 Access to new and priority markets for Canada’s natural resources is enhanced",
                                                                "R8 Canadians are engaged in the future of the new and inclusive resource economy",
                                                                "R9 Enhanced competitiveness of Canada’s natural resource sectors"};



        public static readonly string[] CORE_RESPONSIBILITY1 = { "R1 Canadians have access to cutting-edge research to inform decisions on the management  of natural resources",
"R2 Communities and officials have the tools to safeguard Canadians from natural hazards and explosives",
"R3 Communities and industries are adapting to climate change"};

        public static readonly string[] CORE_RESPONSIBILITY2 = { "R4 Natural resource sectors are innovative",
"R5 Clean technologies and energy efficiencies enhance economic performance",
"R6 Canada’s natural resources are sustainable"};

        public static readonly string[] CORE_RESPONSIBILITY3 = {"R7 Access to new and priority markets for Canada’s natural resources is enhanced",
"R8 Canadians are engaged in the future of the new and inclusive resource economy",
"R9 Enhanced competitiveness of Canada’s natural resource sectors"};
        //   "Core Responsibility 1", "Core Responsibility 2", "Core Responsibility 3", "Internal Services" };
        public static readonly string[] CORE_RESPONSIBILITIES = { "Natural Resource Science and Risk Mitigation", "Innovative and Sustainable Natural Resource Development", "Globally Competitive Natural Resource Sectors", "Internal Services" };

        public static readonly string[] STRATEGIC = {"Accelerate development and adoption of clean technology and transition to a net-zero future in a post-pandemic economic recovery",
"Improve market access and competitiveness for Canada’s resource sectors",
"Advance reconciliation, build relationships and share economic benefits with Indigenous peoples",
        "Promote a diverse and inclusive workforce while supporting resource communities",
        "Protect Canadians from the impacts of natural and human-induced hazards and support climate action"};

        public static readonly string[] STRATEGIC1 = {"Protecting Canadians from the impacts of natural and human-induced hazards",
"Accelerating the adoption of clean technology and supporting the transition to a low-carbon future",
"Advancing reconciliation, building relationships, and sharing economic benefits with Indigenous peoples"};

        public static readonly string[] STRATEGIC2 = {"Accelerating the adoption of clean technology and supporting the transition of a low-carbon future",
"Improving market access and competitiveness in Canada’s resource sectors",
"Supporting resource communities and workers in a low carbon economy",
" Advancing reconciliation, building relationships, and sharing economic benefits with Indigenous peoples"};

        public static readonly string[] STRATEGIC3 = {"Accelerating the adoption of clean technology and supporting the transition of a low-carbon future",
"Improving market access and competitiveness in Canada’s resource sectors",
"Supporting resource communities and workers in a low-carbon economy",
"Advancing reconciliation, building relationships, and sharing economic benefits with Indigenous peoples"
};

        public static readonly string[] COMMITMENTS = {"1 - Support workers and businesses in natural resource sectors that are seeking to export to global markets, including working to complete the twinning of the Trans Mountain Pipeline",
"2 - Work with Partners to implement recommendations of the Generation Energy Council Report, the Canadian Minerals and Metals Plan (CMMP) and strengthen the competitiveness of the forest sector",
"3 - Position Canada as a global leader in clean technology, including critical in minerals",
"4 - Operationalize the plan to plant two billion incremental trees over the next 10 years ",
"5 - Help cities expand and diversify their urban forests and invest in protecting trees from infestations and, when ecologically appropriate, helping to rebuild forests after a wildfire",
"6 - Support research and provide funding so that municipalities have access to domestic sources of climate-resilient and genetically diverse trees that will increase the resilience of Canada’s urban forests",
"7 - Advance legislation to support the future and livelihood of workers and their communities in the transition to a low-carbon global economy",
"8 - Help Canadians make their homes more energy efficient and climate resilient",
"9 - Make ENERGY STAR© certification mandatory for all new home appliances starting in 2022",
"10 - Launch a national competition to create four long-term funds to help attract private capital that can be used for deep retrofits of large buildings such as office towers",
"11 - Install up to 5,000 additional charging stations along the Trans-Canada Highway and other major road networks and in Canada’s urban and rural areas",
"12 - Advance the electrification of Canadian industries through new, zero-carbon clean electricity generation and transmission systems and grid modernization",
"13 - Support the transition of Indigenous communities from reliance on diesel-fueled power to clean, renewable and reliable energy by 2030",
"14 - Ensure the efficient and effective implementation of the Canadian Energy Regulator Act (CERA)",
"15 - Develop a new national benefits-sharing framework for major resource projects on Indigenous territory",
"16 - Complete all flood maps in Canada ",
"17 - Monitor and identify any additional assistance the Polar Continental Shelf Program (PCSP) may require to respond to growing demand",
"18 - Move forward with large-scale building retrofits and the Clean Power Fund that will connect surplus clean power to regions, including projects like Atlantic Loop",
"19 - Help Canadians retrofit their homes and buildings",
"20 - Make zero-emissions vehicles more affordable while investing in more charging stations across the country and ensure Canada is globally competitive building zero-emissions vehicles and batteries",
"21 - Support investments in renewable energy, energy efficiency, energy storage and next-generation clean energy and technology solutions, including in Indigenous communities",
"22 - Implement the Net-Zero Accelerator Fund",
"23 - Develop a national climate change adaptation strategy and invest in reducing the impact of climate-related disasters, such as floods and wildfires, to make communities safer and more resilient",
"24 - Operationalize our plan to use nature-based solutions to fight climate change and stop biodiversity loss, including by planting two billion trees.",
"25 - Create a new Canada Water Agency",
"26 - Support the efforts of foresters to reduce emissions and build resilience as key partners in the fight against climate change",
"27 - Develop a comprehensive blue economy strategy",
};

        public static readonly string[] TRANSFER_PROGRAMS = {"Contributions in support of the Green Construction through Wood Program (voted)",
"Grants and Contributions in support of Clean Technology Challenges (voted)",
"Contributions in support of Energy Efficiency (voted)",
"Contributions in support of Expanding Market Opportunities (voted)",
"Contribution in support of the clean-up of the Gunnar uranium mining facilities (voted)",
"Contributions in support of Clean Growth in Natural Resource Sectors Innovation Program (voted)",
"Contributions in support of the Energy Innovation Program (voted)",
"Contributions in support of Indigenous Advisory and Monitoring Committees for Energy Infrastructure Projects (voted)",
"Contributions in support of Electric Vehicle and Alternative Fuel Infrastructure Deployment (voted)",
"Contributions in support of the Smart Grid Program (voted)",
"Contributions in support of Clean Energy for Rural and Remote Communities (voted)",
"Contributions in support of the Emerging Renewable Power Program (voted)",
"Contributions in support of Spruce Budworm Early Intervention Strategy - Phase II (voted)",
"Contributions in support of the Emissions Reduction Fund",
"Contributions in support of Investments in the Forest Industry Transformation Program (voted)",
"Contributions in support of the Forest Innovation Program (voted)",
"Contributions in support of Mountain Pine Beetle Management in Alberta (voted)",
"Contributions in support of Indigenous Natural Resources Partnerships (voted)",
"Payments to the Newfoundland Offshore Petroleum Resource Revenue Fund (statutory)",
"Contributions to the Canada/Newfoundland and Labrador Offshore Petroleum Board (statutory)",
"Contributions in support of Zero Emission Vehicle Infrastructure (voted)",
"Contributions in support of Accommodation Measures for the Trans Mountain Expansion project (voted)",
"Contributions in support of the Electric Vehicle Infrastructure Demonstration Program (voted)",
"Contributions in support of Climate Change Adaptation (voted)",
"Grants in support of Home Retrofits (voted)",
"Payments to the Nova Scotia Offshore Revenue Account (Statutory)",
"Payment to support students and youth impacted by Covid-19",
"Fund for Safety measures in forest sector operations (COVID-19)"
        };

        public static readonly string[] TRANSFER_PROGRAMS_LESS5 = {"Grants in support of Outreach and Engagement, Energy Efficiency and Energy Innovation (voted)",
"Grants in support of Innovative Solutions Canada (voted)",
"Contributions in support of the Indigenous Consultations Participant Funding Program (voted)",
"Contributions in support of National Risk Profile – Wildfire Risk Assessment (voted)",
"Contributions in support of Research (voted)",
"Contributions in support of the GeoConnections Program (voted)",
"Grants in support of Geoscience (voted)",
"Grants in support of Improving Diversity in the Canadian Forest Sector Workforce (voted)",
"Contributions is support of the Forest Research Institute Initiative (voted)",
"Contributions in support of Indigenous Economic Development (voted)",
"Payments to the Canada-Nova Scotia Offshore Petroleum Board (statutory)",
"Youth Employment and Skills Strategy – Science and Technology Internship Program (voted)",
"Contributions in support of Indigenous participation in dialogues (voted)",
"Grants and Contributions in support of Oil Spill Recovery Technology under the program entitled Incentives to Develop Oil Spill Recovery Technologies (voted)",
"Contributions in support of Cyber Security and Critical Energy Infrastructure Protection (voted)",
"Contributions in support of Wildland Fire Resilience (voted)",
"Contributions in support of Earthquake Early Warning (voted)",
"Climate Action Support Payments (Energy Manager Program and Clean Energy for Rural and Remote Communities Program)"};

        public static readonly string[] HORIZONTAL_INITIATIVES = { "CGCC – Clean Growth and Climate Change (ECCC)",
"Emergency Management Strategy (PSC)",
"FCSAP – Federal Contaminated Sites Action Plan (ECCC)",
"IICP – Investing in Infrastructure Canada Plan (INFC)",
"IARP – Impact Assessment and Regulatory Processes (IAAC)",
"NCSS - National Cyber Security Strategy (PSC)",
"Nature Legacy for Canada (ECCC)",
"OPP – Oceans Protection Plan (TC)",
"YESS - Youth Employment Skills Strategy (ESDC)",
};
        public static readonly string[] GOC_OUTCOMES = {"Economic Affairs - income security and employment for Canadians",
"Economic Affairs - Strong economic growth",
"Economic Affairs - an innovative and knowledge based economy",
"Economic Affairs - a clean and healthy environment",
"Economic Affairs - a fair and secure market place",
"Social Affairs - healthy Canadians",
"Social Affairs - a safe and secure Canada",
"Social Affairs - a diverse society that promotes linguistic duality and social inclusion",
"Social Affairs - a vibrant Canadian culture and heritage",
"International Affairs - a safe and secure world through international engagement",
"International Affairs - global poverty reduction through international sustainable development",
"International Affairs - a strong and mutually beneficial North American partnership",
"International Affairs - a prosperous Canada through global commerce",
"Government Affairs - strong and independent democratic institutions",
"Government Affairs - a transparent accountable and responsive federal government",
"Government Affairs - well-managed and efficient government operations",
 };
        public static readonly string[] INTERVENTION = {"Programs / Services for Canadians",
"Regulation / Legislation",
"Management / Oversight of Federal Activities",
"Grant",
"Contribution",
"Other Transfer Payments",
"Enterprise-Wide Program / Service",
"Safety or Security Program / Service" };
        public static readonly string[] TARGET_GROUPS = {"Economic Segments - Agriculture industry",
"Economic Segments - Energy and/or utilities sector",
"Economic Segments - Entrepreneurs",
"Economic Segments - Finance and/or insurance sectors",
"Economic Segments - Fisheries and Aquaculture",
"Economic Segments - Forestry industry",
"Economic Segments - Hospitality and/or food services industry",
"Economic Segments - Housing sector",
"Economic Segments - Import / export sectors",
"Economic Segments - Indigenous/northern businesses",
"Economic Segments - Infrastructure",
"Economic Segments - Large sized businesses",
"Economic Segments - Manufacturing industry",
"Economic Segments - Medium sized businesses",
"Economic Segments - Mining, and/or oil & gas exploration industries",
"Economic Segments - Movie, television, and/or publishing sectors",
"Economic Segments - Performing arts sector",
"Economic Segments - Retail industry",
"Economic Segments - Science and technology industry",
"Economic Segments - Small and Medium Enterprises",
"Economic Segments - Small sized businesses",
"Economic Segments - Sports and/or recreation industry",
"Economic Segments - Telecommunications sector",
"Economic Segments - Training and/or educational sectors",
"Economic Segments - Transportation industry",
"Environmental - Contaminated sites",
"Environmental - Ecological systems and/or natural habitats",
"Environmental - Greenhouse gas emitters",
"Environmental - Species at risk and/or invasive species",
"Environmental - Water treatment / distribution facilities",
"Foreign Entities - Civil society in developing countries / regions",
"Foreign Entities - Families in developing countries / regions",
"Foreign Entities - Foreign governments",
"Foreign Entities - Government institutions in developing countries / regions",
"Foreign Entities - International organizations and/or alliances",
"Foreign Entities - Private sector / businesses in developing countries / regions",
"Internal to Government - Canadian Forces",
"Internal to Government - Federal departments and/or agencies",
"Internal to Government - Federal public service",
"Internal to Government - Program(s) within the same department or agency",
"Internal to Government - Public servants",
"Non-Profit Institutions and Organizations - Colleges and/or universities",
"Non-Profit Institutions and Organizations - Health care and/or social assistance sectors",
"Non-Profit Institutions and Organizations - Heritage institutions",
"Non-Profit Institutions and Organizations - Non-governmental organizations (NGO)",
"Persons - Artists and/or performers",
"Persons - Athletes and/or coaches",
"Persons - Canadians travelling, working, studying, and/or living abroad",
"Persons - Children",
"Persons - Consumers",
"Persons - Dependants of Military and Law Enforcement Veterans",
"Persons - Detained and/or formerly incarcerated individuals",
"Persons - Families",
"Persons - Farmers",
"Persons - Foreign and/or migrant workers",
"Persons - Foreign investors and/or foreign entrepreneurs",
"Persons - General public",
"Persons - Health care professionals",
"Persons - Immigrants and/or refugees",
"Persons - Indigenous people",
"Persons - International students",
"Persons - Language minority communities",
"Persons - Law enforcement officials",
"Persons - Legal professionals",
"Persons - Low-income individuals and/or families",
"Persons - Members of Parliament",
"Persons - Military personnel",
"Persons - Persons with disabilities",
"Persons - Scientific researchers",
"Persons - Seniors",
"Persons - Socio-economic researchers",
"Persons - Students",
"Persons - Tourists and/or foreign visitors",
"Persons - Unemployed",
"Persons - Veterans",
"Persons - Victims",
"Persons - Violators of regulations and/or laws",
"Persons - Voters",
"Persons - Women",
"Persons - Workers",
"Persons - Youth",
"Provinces, Territories and Communities - Indigenous Band, Tribal Council, Nation and/or Alliance",
"Provinces, Territories and Communities - Local and/or regional communities",
"Provinces, Territories and Communities - Municipal governments",
"Provinces, Territories and Communities - Northern communities",
"Provinces, Territories and Communities - Provincial & territorial governments",
"Provinces, Territories and Communities - Rural communities",
"Provinces, Territories and Communities - Urban communities",
 };
        public static readonly string[] GOC_TAGS1 = {
"Crown Corporations", 
"Democratic Institutions",
"Economic Development",
"Employment and Income Security",
"Environment",
"Government Operations",
"Health",
"Heritage and Culture",
"Internal Services",
"International Development",
"International Engagement",
"International Trade and Investment",
"Market Integrity, regulation and competition",
"North America Partnership",
"Research and Development",
"Safety and Security",
"Social Inclusion",
"Transparency and Accountability",
};

        public static readonly string[] GOC_TAGS = {"1 - Children",
"2 - Youth",
"3 - Seniors",
"4 - Families",
"5 - Women",
"6 - Indigenous people",
"7 - Disabled persons",
"8 - Students",
"9 - Detained and/or formerly incarcerated individuals",
"10 - Violators of regulations and/or laws",
"11 - Victims",
"12 - Military personnel",
"13 - Veterans",
"14 - Workers",
"15 - Voters",
"16 - Consumers",
"17 - Unemployed",
"18 - Low-income individuals and/or families",
"19 - Scientific researchers",
"20 - Socio-economic researchers",
"21 - Health care professionals",
"22 - Law enforcement officials",
"23 - Legal professionals",
"24 - Artists and/or performers",
"25 - Athletes and/or coaches",
"26 - Farmers",
"27 - Members of Parliament",
"28 - Language minorities",
"29 - Canadians travelling, working, studying, and/or living abroad",
"30 - Immigrants and/or refugees",
"31 - International students",
"32 - Tourists and/or foreign visitors",
"33 - Foreign and/or migrant workers",
"34 - Foreign investors and/or foreign entrepreneurs",
"35 - General public",
"36 - Dependants of Military and Law Enforcement Veterans",
"37 - Non-governmental organizations (NGO)",
"38 - Health care and/or social assistance sectors",
"39 - Heritage institutions",
"40 - Colleges and/or universities",
"41 - Agriculture industry",
"42 - Forestry industry",
"43 - Mining, and/or oil & gas exploration industries",
"44 - Energy and/or utilities sector",
"45 - Manufacturing industry",
"46 - Import / export sectors",
"47 - Retail industry",
"48 - Transportation industry",
"49 - Movie, television, and/or publishing sectors",
"50 - Telecommunications sector",
"51 - Science and technology industry",
"52 - Finance and/or insurance sectors",
"53 - Housing sector",
"54 - Sports and/or recreation industry",
"55 - Hospitality and/or food services industry",
"56 - Training and/or educational sectors",
"57 - Performing arts sector",
"58 - Infrastructure",
"59 - Indigenous/northern businesses",
"60 - Small and Medium Enterprises",
"61 - Large sized businesses",
"62 - Families in developing countries / regions",
"63 - Private sector / businesses in developing countries / regions",
"64 - Civil society in developing countries / regions",
"65 - International organizations and/or alliances",
"66 - Foreign governments",
"67 - Provincial & territorial governments",
"68 - Urban communities",
"69 - Rural communities",
"70 - Northern communities",
"71 - Local and/or regional communities",
"72 - Municipal governments",
"73 - Indigenous Band, Tribal Council, Nation and/or Alliance",
"74 - Federal departments and/or agencies",
"75 - Program(s) within the same department or agency",
"76 - Public Servants",
"77 - Canadian Forces",
"78 - Contaminated sites",
"79 - Greenhouse gas emitters",
"80 - Water treatment / distribution facilities",
"81 - Ecological systems and/or natural habitats",
"82 - Species at risk and/or invasive species" };
        public static readonly string[] GOC_CLASSIFICATION = {
        "Economic affairs n.e.c.",
        "Electricity",
        "Executive and legislative organs",
        "Forestry",
        "Fuel and energy n.e.c",
        "General economic and commercial affairs",
        "General public services n.e.c",
        "General public services n.e.c.",
        "General purpose transfers to Provincial and Territorial governments",
        "Mining of mineral resources other than mineral fuels",
        "Overall planning and statistical services",
        "Petroleum and natural gas",
        "Protection of biodiversity and landscape",
        "R&D Fuel and energy",
        "R&D Mining, manufacturing, and construction",
        "R&D Other industries"
        };
    }
}
