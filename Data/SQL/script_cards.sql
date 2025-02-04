USE [web-bite]
GO
/****** Object:  Table [dbo].[CardGameCard]    Script Date: 03.10.2024 13:20:36 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CardGameCard](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CardName] [nvarchar](max) NOT NULL,
	[AttackValue] [int] NOT NULL,
	[DefenseValue] [int] NOT NULL,
	[Label] [nvarchar](max) NOT NULL,
	[SpecialAbility] [int] NOT NULL,
	[FtpImageUrl] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_CardGameCard] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[CardGameCard] ON 

INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (1, N'GORĄCA HERBATA', 0, 0, N'+2 DO OBRONY DLA WSZYSTKICH KART', 1, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (2, N'ROSÓŁ', 0, 0, N'+2 DO ATAKU DLA WSZYSTKICH KART', 2, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (3, N'SKÓRKA OD BANANA', 0, 0, N'WYŁĄCZ KARTĘ PRZECIWNIKA Z NAJWIĘKSZĄ WARTOŚCIĄ ATAKU ', 3, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (4, N'INDYJSKI STREET FOOD', 0, 0, N'WYŁĄCZ KARTĘ PRZECIWNIKA Z NAJWIĘKSZĄ WARTOŚCIĄ OBRONY ', 4, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (5, N'CHECK ENGINE', 0, 0, N'-2 DO OBRONY DLA WSZYSTKICH KART PRZECIWNIKA', 5, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (7, N'SCHODY NA 3 PIĘTRO', 0, 0, N'-2 DO ATAKU DLA WSZYSTKICH KART PRZECIWNIKA', 6, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (8, N'THE BOYS', 0, 0, N'PODWOJENIE BAZOWEJ WARTOŚCI ATAKU SĄSIEDNICH KART', 7, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (9, N'SHIELD WALL', 0, 0, N'PODWOJENIE BAZOWEJ WARTOŚCI OBRONY SĄSIEDNICH KART', 8, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (10, N'POZYCJA EMBRIONALNA', 0, 10, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (11, N'POZYCJA EMBRIONALNA', 0, 10, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (13, N'NAGRZANY KOC', 1, 9, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (14, N'NAGRZANY KOC', 1, 9, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (15, N'STERTA UBRAŃ', 2, 8, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (16, N'STERTA UBRAŃ', 2, 8, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (17, N'ZBUK', 3, 7, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (18, N'ZBUK', 3, 7, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (19, N'ŻONOBIJKA', 4, 6, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (20, N'ŻONOBIJKA', 4, 6, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (22, N'ĆWIERĆSTRZAŁOWIEC', 5, 5, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (23, N'ĆWIERĆSTRZAŁOWIEC', 5, 5, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (25, N'BRUDNA SKARPETA', 6, 4, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (27, N'BRUDNA SKARPETA', 6, 4, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (28, N'POKROJONA CEBULA', 7, 3, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (29, N'POKROJONA CEBULA', 7, 3, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (30, N'WYBITE SZAMBO', 8, 2, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (31, N'WYBITE SZAMBO', 8, 2, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (32, N'SPŁATA KREDYTU', 9, 1, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (33, N'SPŁATA KREDYTU', 9, 1, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (34, N'LA CHANCLA', 10, 0, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
INSERT [dbo].[CardGameCard] ([Id], [CardName], [AttackValue], [DefenseValue], [Label], [SpecialAbility], [FtpImageUrl]) VALUES (35, N'LA CHANCLA', 10, 0, N'JEDNOSTKA', 0, N'assets/images/card-images/dirty-socks.webp')
SET IDENTITY_INSERT [dbo].[CardGameCard] OFF
GO
ALTER TABLE [dbo].[CardGameCard] ADD  DEFAULT (N'') FOR [FtpImageUrl]
GO
