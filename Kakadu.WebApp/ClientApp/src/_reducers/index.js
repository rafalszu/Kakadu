import { combineReducers } from 'redux';

import { authentication } from './authentication.reducer';
import { users } from './users.reducer';
import { alert } from './alert.reducer';
import { services } from './services.reducer';
import { knownRoute } from './knownroute.reducer';

const rootReducer = combineReducers({
  authentication,
  users,
  services,
  knownRoute,
  alert
});

export default rootReducer;