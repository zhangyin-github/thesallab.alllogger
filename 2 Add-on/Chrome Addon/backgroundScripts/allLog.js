/******** Websocket ********/

let allLogSender = {
    send: function (o) {
        websocket.send(o);
    },

    sendRequest: function (o, sequenceId) {
        o.action = "request";
        o.sequenceId = sequenceId;
        allLogSender.send(o);
    },

    sendActive: function (o, sequenceId) {
        o.action = "active";
        o.sequenceId = sequenceId;
        allLogSender.send(o, sequenceId);
    }
};

/******** Request ********/

chrome.webNavigation.onCreatedNavigationTarget.addListener((details) => {
    chrome.tabs.get(details.sourceTabId, (sourceTab) => {
        NavigationEventHandler.createdNavigationTarget(details.url,
            details.tabId, details.sourceTabId, sourceTab.url);
    });
});

chrome.webNavigation.onBeforeNavigate.addListener((details) => {
    chrome.tabs.get(details.tabId, (sourceTab) => {
        NavigationEventHandler.beforeNavigate(details.url, details.tabId,
            sourceTab.url);
    });
});

chrome.webNavigation.onCommitted.addListener((details) => {
    NavigationEventHandler.committed(details.url, details.tabId,
        details.transitionType, details.transitionQualifiers);
});


NavigationEventHandler = {
    createdNavigationTarget: (url, tabId, sourceTabId, referUrl) => {
        if (NavigationEventHandler.tabs[tabId] == null) {
            NavigationEventHandler.tabs[tabId] = {};
        }

        NavigationEventHandler.tabs[tabId].createdNavigationTarget = {
            url: url, tabId: tabId, sourceTabId: sourceTabId, referUrl: referUrl
        };

        NavigationEventHandler.tabs[tabId].first = true;
    },

    beforeNavigate: (url, tabId, referUrl) => {
        if (NavigationEventHandler.tabs[tabId] == null) {
            NavigationEventHandler.tabs[tabId] = {};
        }

        NavigationEventHandler.tabs[tabId].beforeNavigate =
            {url: url, tabId: tabId, referUrl: referUrl};
    },

    committed: (url, tabId, type, qlf) => {
        if (NavigationEventHandler.tabs[tabId] == null) {
            NavigationEventHandler.tabs[tabId] = {};
            NavigationEventHandler.beforeNavigate(url, tabId, url);
        }

        NavigationEventHandler.tabs[tabId].committed =
            {url: url, tabId: tabId, type: type, qlf: qlf};
        NavigationEventHandler.process(tabId);
    },

    process: (tabId) => {
        let tab = NavigationEventHandler.tabs[tabId];

        if (tab.first) {
            if (tab.createdNavigationTarget.url == tab.beforeNavigate.url) {
                tab.sequence = NavigationEventHandler.sequence(
                    tab.createdNavigationTarget.sourceTabId);

                let request = {
                    tabId: tabId,
                    url: tab.committed.url,
                    sourceTabId: tab.createdNavigationTarget.sourceTabId,
                    refer: tab.createdNavigationTarget.referUrl,
                    timestamp: Date.now()
                };
                allLogSender.sendRequest(request, tab.sequence.id);

                tab.first = false;
            } else {
                NavigationEventHandler.sequence(tabId);

                let request = {
                    tabId: tabId,
                    url: tab.committed.url,
                    refer: tab.beforeNavigate.referUrl,
                    timestamp: Date.now()
                };
                allLogSender.sendRequest(request, tab.sequence.id);

                tab.first = false;
            }

        } else if (tab.committed.type == "typed" || tab.committed.type ==
            "auto_bookmark" || tab.committed.type == "generated" ||
            tab.committed.type == "start_page" || tab.committed.type ==
            "keyword" || tab.committed.type == "keyword_generated") {
            tab.sequence = null;
            NavigationEventHandler.sequence(tabId);

            let request = {
                tabId: tabId, url: tab.committed.url, timestamp: Date.now()
            };
            allLogSender.sendRequest(request, tab.sequence.id);
        } else if (tab.committed.type == "link" || tab.committed.type ==
            "form_submit") {
            NavigationEventHandler.sequence(tabId);

            let request = {
                tabId: tabId,
                url: tab.committed.url,
                refer: tab.beforeNavigate.referUrl,
                timestamp: Date.now()
            };
            allLogSender.sendRequest(request, tab.sequence.id);
        }
    },

    sequence: (tabId) => {
        if (NavigationEventHandler.tabs[tabId] == null) {
            return null;
        }

        let sequence = NavigationEventHandler.tabs[tabId].sequence;
        if (sequence == null) {
            let sequence = {};
            NavigationEventHandler.tabs[tabId].sequence = sequence;
            NavigationEventHandler.sequences.push(sequence);
            sequence.id = NavigationEventHandler.sequences.push(sequence);
        }

        return sequence;
    },

    tabs: [], sequences: []
};

/******** Active ********/

chrome.tabs.onActivated.addListener((activeInfo) => {
    chrome.tabs.get(activeInfo.tabId).then((tabInfo) => {
        let active = {
            tabId: tabInfo.id, url: tabInfo.url, timestamp: Date.now()
        };
        let seq = NavigationEventHandler.sequence(activeInfo.tabId);

        if (seq == null) {
            return;
        }
        allLogSender.sendActive(active, seq.id);
    }, (error) => {
    });
});

chrome.tabs.onUpdated.addListener((tabId, changeInfo, tabInfo) => {
    if (changeInfo.status && changeInfo.status == "complete" &&
        tabInfo.active) {
        let active = {
            tabId: tabInfo.tabId, url: tabInfo.url, timestamp: Date.now()
        };

        let seq = NavigationEventHandler.sequence(tabId);
        if (seq == null) {
            return;
        }

        allLogSender.sendActive(active, seq.id);
        return true;
    }
});