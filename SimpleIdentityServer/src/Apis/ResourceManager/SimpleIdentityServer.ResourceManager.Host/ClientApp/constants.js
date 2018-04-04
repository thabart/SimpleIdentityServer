module.exports = {
    baseUrl: process.env.BASE_URL,
    eventSourceUrl: process.env.EVT_SOURCE_URL,
    apiUrl: process.env.API_URL,
    events: {
        USER_LOGGED_IN: 'USER_LOGGED_IN',
        USER_LOGGED_OUT: 'USER_LOGGED_OUT'
    }
};