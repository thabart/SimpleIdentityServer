module.exports = {
    baseUrl: process.env.BASE_URL,
    eventSourceUrl: process.env.EVT_SOURCE_URL,
    apiUrl: process.env.API_URL,
    openIdUrl: process.env.OPENID_URL,
    clientId: 'ResourceManagerClientId',
    events: {
        USER_LOGGED_IN: 'USER_LOGGED_IN',
        USER_LOGGED_OUT: 'USER_LOGGED_OUT'
    }
};