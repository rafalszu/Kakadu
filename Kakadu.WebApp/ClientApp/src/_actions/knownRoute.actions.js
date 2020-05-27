import { knownRouteConstants } from '../_constants';

export const knownRouteActions = {
    selectKnownRoute
};

function selectKnownRoute(route) {
    return dispatch => {
        dispatch(select(route));
    };

    function select(route) { return { type: knownRouteConstants.SELECT_ROUTE, payload: route }}
}