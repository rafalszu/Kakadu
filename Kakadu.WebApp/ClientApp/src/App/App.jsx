import React from 'react';
import { Router, Route } from 'react-router-dom';
import { connect } from 'react-redux';

import { history } from '../_helpers';
import { alertActions } from '../_actions';
import { PrivateRoute } from '../Components';
import { Home } from '../Components/Home';
import { Login } from '../Components/Login';
import { Layout } from '../Components/Layout';
import { List as ServicesList, Edit as ServiceEdit, Add as ServiceAdd } from '../Components/Services';

import { library } from '@fortawesome/fontawesome-svg-core'
import { faStop, faCircle, faEdit, faTrashAlt } from '@fortawesome/free-solid-svg-icons'

import '../custom.css'

library.add(faTrashAlt, faEdit, faCircle, faStop)

class App extends React.Component {
    constructor(props) {
        super(props);

        const { dispatch } = this.props;
        history.listen((location, action) => {
            // clear alert on location change
            dispatch(alertActions.clear());
        });
    }

    render() {
        const { alert } = this.props;
        return (
            <Router history={history}>
                <Layout>
                    {alert.message &&
                        <div className={`alert ${alert.type}`}>{alert.message}</div>
                    }
                    <div>
                        <PrivateRoute exact path="/" component={Home} />
                        <PrivateRoute exact path="/services" component={ServicesList} />
                        <PrivateRoute exact path="/services/edit/:serviceCode" component={ServiceEdit} />
                        <PrivateRoute exact path="/services/add" component={ServiceAdd} />
                        <Route path="/login" component={Login} />
                    </div>
                </Layout>
            </Router>
        );
    }
}

function mapStateToProps(state) {
    const { alert } = state;
    return {
        alert
    };
}

const connectedApp = connect(mapStateToProps)(App);
export { connectedApp as App }; 