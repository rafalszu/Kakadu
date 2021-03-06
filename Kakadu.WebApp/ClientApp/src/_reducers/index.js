import { combineReducers } from 'redux';

import { authentication } from './authentication.reducer';
import { users } from './users.reducer';
import { alert } from './alert.reducer';
import { services } from './services.reducer';
import { knownRoute } from './knownroute.reducer';
import { knownRouteReplies } from './knownRouteReplies.reducer';
import { knownRouteReply } from './knownRouteReply.reducer';

const rootReducer = combineReducers({
  authentication,
  users,
  services,
  knownRoute,
  knownRouteReplies,
  knownRouteReply,
  alert
});

export default rootReducer;