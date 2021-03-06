import Vue from 'vue'
import Router from 'vue-router'
import store from '../store/store' //引入状态管理
import NProgress from 'nprogress' //引入进度条组件 cnpm install nprogress --save
import 'nprogress/nprogress.css' 
Vue.use(Router)

  /**
    *@parma {String} name 文件夹名称
    *@parma {String} component 视图组件名称
    */
   const getComponent = (name,component) => () => import(`@/views/${name}/${component}.vue`);
   const getComponentByName = name => () => import(`@/views/${name}.vue`);


const myRouter=new Router({
  routes: [
   
    {
      path: '/',
      redirect: '/home',
      component: getComponent('login','index')
    },
    {
      path: '/login',
      name: 'login',
      component: getComponent('login','index')
    },
    {
      path: '/',
      component:getComponent('layout','Layout'),
      children:[{
        path:'/home',
        name:'home',
        component: getComponent('home','index'),
        meta:{title:'首页'}
      },
      {
        path: '/element',
        component: getComponent('icons', 'elementIcom'),
        meta: {
            title: '饿了吗图标'
        }
      },
     {
        path: '/iconfont',
        component: getComponent('icons', 'iconfont'),
        meta: {
            title: 'icon图标'
        }
      },
      {
        path:'/editor',
        component: getComponent('components','editor'),
        name:'editor',
        meta:{title:'富文本编译器'}
      },
      {
        path:'/countTo',
        component: getComponent('components','countTo'),
        name:'countTo',
        meta:{title:'数字滚动'}
      },
      {
        path:'/tree',
        component: getComponent('components','tree'),
        name:'tree',
        meta:{title:'自定义树'}
      },
      {
        path:'/treeTable',
        component: getComponent('components','treeTable'),
        name:'treeTable',
        meta:{title:'表格树'}
      },
      {
        path:'/treeSelect',
        component: getComponent('components','treeSelect'),
        name:'treeSelect',
        meta:{title:'下拉树'}
      },
      {
        path:'/draglist',
        component: getComponent('draggable','draglist'),
        name:'draglist',
        meta:{title:'拖拽列表'}
      },
      {
        path:'/dragtable',
        component: getComponent('draggable','dragtable'),
        name:'dragtable',
        meta:{title:'拖拽表格'}
      },
      {
        path:'/cricle',
        component: getComponent('charts','cricle'),
        name:'cricle',
        meta:{title:'饼图'}
      },
      {
        path:'/permission',
        component: getComponent('permission','permission'),
        name:'permission',
        meta:{title:'权限测试'}
      },
      {
        path:'/403',  //name和path保持一致
        component: getComponent('error','403'),
        name:'403',
        meta:{title:'403错误'}
      },
      {
        path:'/404',
        component: getComponent('error','404'),
        name:'404',
        meta:{title:'404错误'}
      },
      {
        path:'/user',
        component: getComponent('systemManager','user'),
        name:'user',
        meta:{title:'用户管理'}
      },

      //表单测试
      {
        path:'/page1',
        name:'form',
        component:getComponentByName('page1'),
        meta:{title:'表格树'}
      },
      //布局测试
      {
        path:'/pageOne',
        name:'pageOne',
        component:getComponent('layoutTest',"pageOne"),
        meta:{title:'单一分栏'}
      },
      {
        path:'/pageTwo',
        name:'pageTwo',
        component:getComponent('layoutTest',"pageTwo"),
        meta:{title:'单一分栏分隔'}
      },
      {
        path:'/pageThree',
        name:'pageThree',
        component:getComponent('layoutTest',"pageThree"),
        meta:{title:'分栏混合版'}
      },
      {
        path:'/pageFour',
        name:'pageFour',
        component:getComponent('layoutTest',"pageFour"),
        meta:{title:'分栏偏移'}
      },
      {
        path:'/pageFive',
        name:'pageFive',
        component:getComponent('layoutTest',"pageFive"),
        meta:{title:'对齐方式'}
      },
      {
        path:'/pageSix',
        name:'pageSix',
        component:getComponent('layoutTest',"pageSix"),
        meta:{title:'响应式布局'}
      },
      {
        path:'/containerOne',
        name:'containerOne',
        component:getComponent('containerTest',"containerOne"),
        meta:{title:'布局容器'}
      },
    ]
    }
  ]
})

     //判断是否存在token,目前没有token,先禁掉
    //  myRouter.beforeEach((to,from,next)=>{
    //   NProgress.start();
    //   if (to.path !== '/login' && store.state.token=='') {
    //      NProgress.done(); // 结束Progress
    //      next('/login');    //跳转登陆
    //   } else {
    //      NProgress.done(); // 结束Progress
    //      next();
    //   }
    //   if (to.meta.roles) {
    //       to.meta.roles.includes(...store.getters.roles) ? next() : next('/404')
    //   } else {
    //       next();
    //   }
    // })
    export default myRouter

