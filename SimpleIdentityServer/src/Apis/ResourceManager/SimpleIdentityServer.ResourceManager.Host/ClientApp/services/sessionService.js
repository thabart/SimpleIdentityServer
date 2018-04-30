var sessionName = "ehealth_website",
    createDateTimeKey = "create_datetime";
var _profile = {};
module.exports = {
    /* Get the session */
    getSession: function () {
        var value = localStorage.getItem(sessionName);
        if (!value || value == null) {
            return null;
        }

        return JSON.parse(value);
    },
    /* Set the session */
    setSession: function (value) {
        value[createDateTimeKey] = new Date().toUTCString();
        localStorage.setItem(sessionName, JSON.stringify(value));
    },
    /* Remove the session */
    remove: function () {
        localStorage.removeItem(sessionName);
    },
    /* Check the identity token is expired */
    isExpired: function () {
        var session = this.getSession();
        if (session === null || !session) {
            return true;
        }

        var endDate = new Date(session[createDateTimeKey]);
        endDate.setSeconds(session["expires_in"] + endDate.getSeconds());
        var currentDate = new Date();
        return currentDate.getTime() > endDate.getTime();
    }
};