-- Déclare la table principale de l'addon
Astralith = Astralith or {}
local waypoints = Astralith.waypoints or {}
Astralith.waypoints = waypoints
local pins = {} -- Table pour stocker les pins
local guildMembers = {} -- Table pour stocker les informations des membres de la guilde

-- Chargement de la bibliothèque HereBeDragons
local HBD = LibStub("HereBeDragons-2.0")
local HBDPins = LibStub("HereBeDragons-Pins-2.0")

-- Intervalle de mise à jour
local updateInterval = 2 -- En secondes
local movementThreshold = 0.005 -- 5% de la carte (environ 5 mètres)
local timeSinceLastUpdate = 0
local lastSentPosition = { x = nil, y = nil, mapID = nil } -- Dernière position envoyée

-- Fonction pour calculer la distance entre deux points
local function CalculateDistance(x1, y1, x2, y2)
    if not x1 or not y1 or not x2 or not y2 then return math.huge end
    return math.sqrt((x2 - x1)^2 + (y2 - y1)^2)
end

-- Préfixe unique pour l'addon
local ADDON_PREFIX = "Astralith"

-- Inscription du préfixe pour la communication addon
C_ChatInfo.RegisterAddonMessagePrefix(ADDON_PREFIX)

-- Fonction pour envoyer la position via Addon Message
function Astralith:SendGuildPosition()
    if IsInGuild() then
        local mapID = C_Map.GetBestMapForUnit("player")
        if not mapID then return end

        local position = C_Map.GetPlayerMapPosition(mapID, "player")
        if position then
            local x, y = position:GetXY()
            if x and y then
                -- Vérifie si la position a changé significativement
                if CalculateDistance(x, y, lastSentPosition.x, lastSentPosition.y) > movementThreshold then
                    local name = UnitName("player")
                    local level = UnitLevel("player")
                    local class = UnitClass("player")
                    local rank = "Membre"

                    -- Parcours des membres de la guilde pour trouver le rang
                    for i = 1, GetNumGuildMembers() do
                        local memberName, memberRank = GetGuildRosterInfo(i)
                        if memberName and memberName == name then
                            rank = memberRank
                            break
                        end
                    end

                    local icon = "Interface\\AddOns\\Astralith\\Textures\\ClassIcon_paladin"
                    local message = string.format("%s,%s,%d,%s,%.3f,%.3f,%d,%s", name, rank, level, class, x, y, mapID, icon)
                    C_ChatInfo.SendAddonMessage(ADDON_PREFIX, message, "GUILD")
                    lastSentPosition = { x = x, y = y, mapID = mapID }
                end
            end
        end
    end
end

-- Fonction pour vérifier si le message provient du joueur lui-même
local function IsSelf(sender)
    local playerName = UnitName("player")
    local realmName = GetNormalizedRealmName()
    local fullPlayerName = playerName .. "-" .. realmName
    return sender == fullPlayerName
end

-- Fonction pour traiter les messages reçus via Addon Message
local function OnAddonMessage(prefix, text, channel, sender)
    if prefix == ADDON_PREFIX and not IsSelf(sender) then
        local name, rank, level, class, x, y, mapID, icon = strsplit(",", text)
        --print("Update ", name, " ", rank, " ", level, " ", class, " ", x, " ", y, " ", mapID, " ", icon)
        x, y, mapID, level = tonumber(x), tonumber(y), tonumber(mapID), tonumber(level)

        if x and y and mapID then
            guildMembers[name] = {
                rank = rank,
                level = level,
                class = class,
                x = x,
                y = y,
                mapID = mapID,
                icon = icon,
            }
            Astralith:CreateGuildMemberPin(name)
        end
    end
end

-- Gestion des événements
local frame = CreateFrame("Frame")
frame:RegisterEvent("CHAT_MSG_ADDON")
frame:RegisterEvent("PLAYER_ENTERING_WORLD")
frame:RegisterEvent("ZONE_CHANGED_NEW_AREA")
frame:SetScript("OnEvent", function(self, event, ...)
    if event == "CHAT_MSG_ADDON" then
        local prefix, text, channel, sender = ...
        OnAddonMessage(prefix, text, channel, sender)
    elseif event == "PLAYER_ENTERING_WORLD" or event == "ZONE_CHANGED_NEW_AREA" then
        Astralith:RefreshPins()
    end
end)

-- Mise à jour périodique pour envoyer la position
local updateFrame = CreateFrame("Frame")
updateFrame:SetScript("OnUpdate", function(self, elapsed)
    timeSinceLastUpdate = timeSinceLastUpdate + elapsed
    if timeSinceLastUpdate >= updateInterval then
        Astralith:SendGuildPosition()
        timeSinceLastUpdate = 0
    end
end)

-- Fonction pour créer ou mettre à jour un pin pour un membre de la guilde
function Astralith:CreateGuildMemberPin(memberName)
    local member = guildMembers[memberName]
    if not member then return end

    Astralith:RemovePinsByTitle(memberName)

    self:AddWaypoint(member.mapID, member.x, member.y, memberName)
end

-- Fonction pour rafraîchir les pins dynamiquement
function Astralith:RefreshPins()
    for memberName, _ in pairs(guildMembers) do
        self:CreateGuildMemberPin(memberName)
    end
end

-- Gestion des événements
local frame = CreateFrame("Frame")
frame:RegisterEvent("CHAT_MSG_GUILD")
frame:RegisterEvent("PLAYER_ENTERING_WORLD")
frame:RegisterEvent("ZONE_CHANGED_NEW_AREA")
frame:SetScript("OnEvent", function(self, event, ...)
    if event == "CHAT_MSG_GUILD" then
        local text, sender = ...
        ProcessGuildMessage(text, sender)
    elseif event == "PLAYER_ENTERING_WORLD" or event == "ZONE_CHANGED_NEW_AREA" then
        Astralith:RefreshPins()
    end
end)

-- Mise à jour périodique pour envoyer la position
local updateFrame = CreateFrame("Frame")
updateFrame:SetScript("OnUpdate", function(self, elapsed)
    timeSinceLastUpdate = timeSinceLastUpdate + elapsed
    if timeSinceLastUpdate >= updateInterval then
        Astralith:SendGuildPosition()
        timeSinceLastUpdate = 0
    end
end)

-- Fonction pour créer deux pins (carte et mini-carte)
function Astralith:CreateMapPin(waypoint)
    -- Supprime les anciens pins s'ils existent
    if pins[waypoint] then
        if pins[waypoint].world then
            HBDPins:RemoveWorldMapIcon("Astralith", pins[waypoint].world)
        end
        if pins[waypoint].minimap then
            HBDPins:RemoveMinimapIcon("Astralith", pins[waypoint].minimap)
        end
        pins[waypoint] = nil
    end

    -- Crée un pin pour la carte mondiale
    local worldPin = CreateFrame("Frame", nil, UIParent)
    worldPin:SetSize(16, 16)

    local worldTexture = worldPin:CreateTexture(nil, "OVERLAY")
    worldTexture:SetAllPoints()
    worldTexture:SetTexture("Interface\\AddOns\\Astralith\\Textures\\ClassIcon_paladin") -- Chemin vers une icône personnalisée
    worldPin.texture = worldTexture

    local worldAdded = HBDPins:AddWorldMapIconMap("Astralith", worldPin, waypoint.mapID, waypoint.x, waypoint.y, HBD_PINS_WORLDMAP_SHOW_WORLD) -- HBD_PINS_WORLDMAP_SHOW_PARENT si uniquement locale

    -- Crée un pin pour la mini-carte
    local minimapPin = CreateFrame("Frame", nil, UIParent)
    minimapPin:SetSize(12, 12)

    local minimapTexture = minimapPin:CreateTexture(nil, "OVERLAY")
    minimapTexture:SetAllPoints()
    minimapTexture:SetTexture("Interface\\AddOns\\Astralith\\Textures\\ClassIcon_paladin") -- Chemin vers une icône personnalisée
    minimapPin.texture = minimapTexture

    local minimapAdded = HBDPins:AddMinimapIconMap("Astralith", minimapPin, waypoint.mapID, waypoint.x, waypoint.y, false)

    -- Stocke les deux pins
    pins[waypoint] = {
        world = worldPin,
        minimap = minimapPin,
        title = waypoint.title
    }

    -- Ajoute un tooltip au pin de la carte mondiale
    worldPin:SetScript("OnEnter", function()
        GameTooltip:SetOwner(worldPin, "ANCHOR_RIGHT")
        GameTooltip:SetText(waypoint.title)
        GameTooltip:Show()
    end)
    worldPin:SetScript("OnLeave", function()
        GameTooltip:Hide()
    end)

    -- Ajoute un tooltip au pin de la mini-carte
    minimapPin:SetScript("OnEnter", function()
        GameTooltip:SetOwner(minimapPin, "ANCHOR_RIGHT")
        GameTooltip:SetText(waypoint.title)
        GameTooltip:Show()
    end)
    minimapPin:SetScript("OnLeave", function()
        GameTooltip:Hide()
    end)
end

-- Fonction pour ajouter un waypoint
function Astralith:AddWaypoint(mapID, x, y, title)
    local waypoint = {
        mapID = mapID,
        x = x,
        y = y,
        title = title or "Waypoint",
    }
    table.insert(waypoints, waypoint)

    -- Ajout du pin via HereBeDragons
    self:CreateMapPin(waypoint)
end

-- Fonction pour supprimer tous les waypoints
function Astralith:ClearAllWaypoints()
    for _, pinSet in pairs(pins) do
        if pinSet.world then
            HBDPins:RemoveWorldMapIcon("Astralith", pinSet.world)
        end
        if pinSet.minimap then
            HBDPins:RemoveMinimapIcon("Astralith", pinSet.minimap)
        end
    end
    waypoints = {}
    pins = {}
    print("Tous les waypoints ont été supprimés.")
end

function Astralith:RemovePinsByTitle(title)
    -- Vérifie que le titre est valide
    if not title then
        print("Erreur : aucun titre fourni pour la suppression.")
        return
    end

    -- Parcourt la table `pins`
    for key, pinSet in pairs(pins) do
        if pinSet.title == title then
            -- Supprime les pins de la carte mondiale
            if pinSet.world then
                HBDPins:RemoveWorldMapIcon("Astralith", pinSet.world)
            end

            -- Supprime les pins de la mini-carte
            if pinSet.minimap then
                HBDPins:RemoveMinimapIcon("Astralith", pinSet.minimap)
            end

            -- Retire l'entrée de la table `pins`
            pins[key] = nil
        end
    end
end



-- Commandes slash
SLASH_ASTRALITH1 = "/astr"
SlashCmdList["ASTRALITH"] = function(msg)
    local cmd, arg1, arg2, arg3 = strsplit(" ", msg)
    if cmd == "add" and arg1 and arg2 and arg3 then
        local mapID = tonumber(arg1)
        local x = tonumber(arg2) / 100
        local y = tonumber(arg3) / 100
        if mapID and x and y then
            Astralith:AddWaypoint(mapID, x, y, "Point Custom")
        else
            print("Utilisation : /astr add <mapID> <x> <y>")
        end
    elseif cmd == "clear" then
        Astralith:ClearAllWaypoints()
    elseif cmd == "relo" then
        Astralith:RemovePinsByTitle("Ævi")
    else
        print("Commandes :")
        print("/astr add <mapID> <x> <y> - Ajoute un point")
        print("/astr clear - Supprime tous les points")
        print("/astr relo - Ajoute un point à votre position actuelle")
    end
end
---------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------
-- Ajouter le bouton sur la minimap
function Astralith:CreateMinimapButton()
    local minimapButton = CreateFrame("Button", "AstralithMinimapButton", Minimap)
    minimapButton:SetSize(32, 32)
    minimapButton:SetFrameStrata("MEDIUM")
    minimapButton:SetFrameLevel(8)

    -- Texture du bouton
    local texture = minimapButton:CreateTexture(nil, "BACKGROUND")
    texture:SetTexture("Interface\\AddOns\\Astralith\\Textures\\MinimapIcon")
    texture:SetAllPoints(minimapButton)
    minimapButton.texture = texture

    -- Déplacement du bouton autour de la minimap
    local angle = 10 -- Position initiale
    local function UpdateMinimapButtonPosition()
        local radius = 80
        local x = math.cos(angle) * radius
        local y = math.sin(angle) * radius
        minimapButton:SetPoint("CENTER", Minimap, "CENTER", x, y)
    end
    UpdateMinimapButtonPosition()

    minimapButton:SetScript("OnDragStart", function()
        minimapButton:SetScript("OnUpdate", function()
            local mx, my = GetCursorPosition()
            local px, py = Minimap:GetCenter()
            local scale = Minimap:GetEffectiveScale()
            angle = math.atan2(my / scale - py, mx / scale - px)
            UpdateMinimapButtonPosition()
        end)
    end)
    minimapButton:SetScript("OnDragStop", function()
        minimapButton:SetScript("OnUpdate", nil)
    end)

    -- Clic pour ouvrir la fenêtre de sélection
    minimapButton:SetScript("OnClick", function(_, button)
        if button == "LeftButton" then
            Astralith:ShowTextureSelector()
        end
    end)

    -- Tooltip
    minimapButton:SetScript("OnEnter", function()
        GameTooltip:SetOwner(minimapButton, "ANCHOR_RIGHT")
        GameTooltip:AddLine("Astralith")
        GameTooltip:AddLine("Clic gauche : Sélectionner une texture")
        GameTooltip:Show()
    end)
    minimapButton:SetScript("OnLeave", function()
        GameTooltip:Hide()
    end)
end

-- Créer une fenêtre pour la sélection de texture
function Astralith:ShowTextureSelector()
    if Astralith.textureSelector then
        Astralith.textureSelector:Show()
        return
    end

    local frame = CreateFrame("Frame", "AstralithTextureSelector", UIParent, "BasicFrameTemplateWithInset")
    frame:SetSize(400, 300)
    frame:SetPoint("CENTER")
    frame:SetMovable(true)
    frame:EnableMouse(true)
    frame:RegisterForDrag("LeftButton")
    frame:SetScript("OnDragStart", frame.StartMoving)
    frame:SetScript("OnDragStop", frame.StopMovingOrSizing)
    frame:Hide()

    frame.title = frame:CreateFontString(nil, "OVERLAY", "GameFontHighlight")
    frame.title:SetPoint("TOP", frame, "TOP", 0, -10)
    frame.title:SetText("Sélectionnez une texture")

    local scrollFrame = CreateFrame("ScrollFrame", "AstralithTextureScrollFrame", frame, "UIPanelScrollFrameTemplate")
    scrollFrame:SetPoint("TOPLEFT", 10, -30)
    scrollFrame:SetPoint("BOTTOMRIGHT", -30, 10)

    local content = CreateFrame("Frame", nil, scrollFrame)
    content:SetSize(350, 1000)
    scrollFrame:SetScrollChild(content)

    local textures = {
        "Interface\\AddOns\\Astralith\\Textures\\ClassIcon_paladin",
        "Interface\\AddOns\\Astralith\\Textures\\ClassIcon_mage",
        "Interface\\AddOns\\Astralith\\Textures\\ClassIcon_warrior",
        -- Ajoutez d'autres textures ici
    }

    local function CreateTextureButton(texturePath, yOffset)
        local button = CreateFrame("Button", nil, content)
        button:SetSize(300, 50)
        button:SetPoint("TOP", 0, yOffset)

        local icon = button:CreateTexture(nil, "BACKGROUND")
        icon:SetSize(50, 50)
        icon:SetPoint("LEFT", 5, 0)
        icon:SetTexture(texturePath)

        local label = button:CreateFontString(nil, "OVERLAY", "GameFontHighlight")
        label:SetPoint("LEFT", icon, "RIGHT", 10, 0)
        label:SetText(texturePath:match("([^\\]+)$"))

        button:SetScript("OnClick", function()
            Astralith:SendSelectedTexture(texturePath)
            frame:Hide()
        end)
    end

    local yOffset = -10
    for _, texture in ipairs(textures) do
        CreateTextureButton(texture, yOffset)
        yOffset = yOffset - 60
    end

    Astralith.textureSelector = frame
    frame:Show()
end

-- Envoyer la texture sélectionnée à la guilde
function Astralith:SendSelectedTexture(texturePath)
    local name = UnitName("player")
    local message = string.format("SET_TEXTURE,%s,%s", name, texturePath)
    C_ChatInfo.SendAddonMessage(ADDON_PREFIX, message, "GUILD")
end

-- Traitement des messages reçus pour changer la texture
local function OnAddonMessage(prefix, text, channel, sender)
    if prefix == ADDON_PREFIX and not IsSelf(sender) then
        local command, name, texturePath = strsplit(",", text)
        if command == "SET_TEXTURE" then
            print(name .. " a sélectionné : " .. texturePath)
            guildMembers[name].icon = texturePath
            Astralith:RefreshPins()
        else
            -- Autres traitements
        end
    end
end

-- Initialisation
Astralith:CreateMinimapButton()

