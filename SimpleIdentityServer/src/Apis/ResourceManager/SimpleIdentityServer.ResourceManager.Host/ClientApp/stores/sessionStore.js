var AppDispatcher = require('../appDispatcher');
var EventEmitter = require('events').EventEmitter;
var $ = require('jquery');
import Constants from '../constants';

var _session = {};

function loadSession(session) {
  _session = session;
}

function updateSession(json) {
  _session = $.extend({}, _session, json);
}

var SessionStore = $.extend({} , EventEmitter.prototype, {
  getSession() {
    return _session;
  },
  emitChange: function() {
    this.emit('change');
  },
  addChangeListener: function(callback) {
    this.on('change', callback);
  },
  removeChangeListener: function(callback) {
    this.removeListener('change', callback);
  }
});

AppDispatcher.register(function(payload) {
  switch(payload.actionName) {
    case Constants.events.SESSION_CREATED:
      loadSession(payload.data);
      SessionStore.emitChange();
      break;
    case Constants.events.SESSION_UPDATED:
      updateSession(payload.data);
      SessionStore.emitChange();
      break;
    default:
      return true;
  }

  return true;

});
module.exports = SessionStore;